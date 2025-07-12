// Robust ekgLive.js  –  bara positiv topp detekteras
window.ekgLive=(function(){
const maxPts=150;let chart,buf=[],rr=[],lastR=0,cnt=0;
const smooth=a=>a.length<3?a.at(-1):(a.at(-1)+a.at(-2)+a.at(-3))/3;

/* adaptiv positiv-peakt detektor */
let peakHist=[];
function detectR(s,now){
  if(s>0) peakHist.push(s);
  if(peakHist.length>1000) peakHist.shift();
  const thr=Math.max(...peakHist,1)*0.30;          // 30 % av max
  return s>thr && now-lastR>250;                   // bara positivt
}

function metrics(){
  if(rr.length<1)return[0,0];
  const mean=rr.reduce((a,b)=>a+b)/rr.length;
  const hr=60000/mean;
  if(rr.length<3)return[Math.round(hr),0];
  const diff=rr.slice(1).map((v,i)=>v-rr[i]);
  const rmssd=Math.sqrt(diff.reduce((s,d)=>s+d*d,0)/diff.length);
  return[Math.round(hr),Math.round(rmssd)];
}

async function init(){
  const cvs=document.getElementById("ekgChart");
  chart=new Chart(cvs.getContext("2d"),{type:"line",
    data:{labels:[],datasets:[{data:[],borderColor:"red",borderWidth:1,pointRadius:0,tension:0.3}]},
    options:{animation:false,scales:{x:{display:false},y:{min:-2,max:2,grid:{color:"#eee"}}},plugins:{legend:{display:false}}}});
  const conn=new signalR.HubConnectionBuilder().withUrl("/ekgHub").build();

  conn.on("ReceiveEkgValue",s=>{
      const now=Date.now();cnt++;
      if(detectR(s,now)){
          if(lastR) rr.push(now-lastR);
          if(rr.length>30) rr.shift();
          lastR=now;
          const[hr,hrv]=metrics();
          DotNet.invokeMethodAsync('BlazorApp','EkgMetrics_Update',hr,hrv);
      }
      if(cnt%2===0){
          chart.data.labels.push(now);
          chart.data.datasets[0].data.push(smooth([...buf,s]));
          if(chart.data.labels.length>maxPts){chart.data.labels.shift();chart.data.datasets[0].data.shift();}
          chart.update('none');
      }
      buf.push(s); if(buf.length>3)buf.shift();
  });

  await conn.start();
  console.log("🎉 /ekgHub connected");
}
return{init};
})();

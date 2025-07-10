// wwwroot/ekg.js
let chart;

window.startEkgChart = async function () {
    // Wait for Chart.js + SignalR scripts to load if not bundled
    if (typeof Chart === "undefined" || typeof signalR === "undefined") {
        console.error("Chart.js or SignalR not loaded");
        return;
    }

    const ctx = document.getElementById('ekgChart').getContext('2d');
    const data = {
        labels: [],
        datasets: [{
            label: 'EKG (sim)',
            data: [],
            borderWidth: 1,
            borderColor: 'red',
            pointRadius: 0,
            tension: 0.1,
            fill: false
        }]
    };

    chart = new Chart(ctx, {
        type: 'line',
        data: data,
        options: {
            animation: false,
            responsive: true,
            scales: {
                x: { display: false },
                y: { min: -2, max: 2 }
            },
            plugins: { legend: { display: false } }
        }
    });

    // Connect to SignalR hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/ekgHub")
        .build();

    connection.on("ReceiveEkgValue", (value) => {
        const now = Date.now();
        data.labels.push(now);
        data.datasets[0].data.push(value);

        // Keep max 500 points for performance
        if (data.labels.length > 500) {
            data.labels.shift();
            data.datasets[0].data.shift();
        }
        chart.update('none');
    });

    await connection.start();
};

document.addEventListener("DOMContentLoaded", function () {

    const sidebar = document.getElementById("adminSidebar");
    const sidebarToggle = document.getElementById("sidebarToggle");
    const sidebarClose = document.getElementById("sidebarClose");
    const sidebarOverlay = document.getElementById("sidebarOverlay");

    function openSidebar() {
        sidebar?.classList.add("show");
        sidebarOverlay?.classList.add("show");
        document.body.style.overflow = "hidden";
    }

    function closeSidebar() {
        sidebar?.classList.remove("show");
        sidebarOverlay?.classList.remove("show");
        document.body.style.overflow = "";
    }

    sidebarToggle?.addEventListener("click", openSidebar);
    sidebarClose?.addEventListener("click", closeSidebar);
    sidebarOverlay?.addEventListener("click", closeSidebar);

    window.addEventListener("resize", function () {
        if (window.innerWidth > 900) {
            closeSidebar();
        }
    });

    // Aktif sidebar menüsünü mevcut URL'e göre belirle
    const currentPath = window.location.pathname.toLowerCase();

    document.querySelectorAll(".sidebar-link").forEach(link => {

        const href = link.getAttribute("href");

        if (!href || href === "#")
            return;

        const linkPath = new URL(link.href).pathname.toLowerCase();

        if (
            currentPath === linkPath ||
            (linkPath !== "/admin" && currentPath.startsWith(linkPath))
        ) {
            link.classList.add("active");
        }
    });
});


function initializeDashboardCharts() {

    const salesCanvas = document.getElementById("salesChart");
    const categoryCanvas = document.getElementById("categoryChart");

    if (salesCanvas) {

        new Chart(salesCanvas, {
            type: "line",

            data: {
                labels: [
                    "Pazartesi",
                    "Salı",
                    "Çarşamba",
                    "Perşembe",
                    "Cuma",
                    "Cumartesi",
                    "Pazar"
                ],

                datasets: [
                    {
                        label: "Satış",
                        data: [120, 165, 142, 198, 184, 235, 218],
                        borderColor: "#347c3a",
                        backgroundColor: "rgba(52, 124, 58, 0.08)",
                        fill: true,
                        tension: 0.4,
                        borderWidth: 2,
                        pointRadius: 0,
                        pointHoverRadius: 5
                    },

                    {
                        label: "Gelir",
                        data: [90, 115, 108, 148, 135, 175, 168],
                        borderColor: "#9acb9e",
                        backgroundColor: "transparent",
                        tension: 0.4,
                        borderWidth: 2,
                        borderDash: [5, 5],
                        pointRadius: 0,
                        pointHoverRadius: 5
                    }
                ]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                interaction: {
                    intersect: false,
                    mode: "index"
                },

                plugins: {
                    legend: {
                        display: false
                    },

                    tooltip: {
                        padding: 10,
                        cornerRadius: 8,
                        titleFont: {
                            size: 10
                        },
                        bodyFont: {
                            size: 9
                        }
                    }
                },

                scales: {
                    x: {
                        grid: {
                            display: false
                        },

                        border: {
                            display: false
                        },

                        ticks: {
                            color: "#9aa39d",
                            font: {
                                size: 8
                            }
                        }
                    },

                    y: {
                        beginAtZero: true,

                        border: {
                            display: false
                        },

                        grid: {
                            color: "#eef1ef"
                        },

                        ticks: {
                            color: "#9aa39d",
                            font: {
                                size: 8
                            }
                        }
                    }
                }
            }
        });
    }


    if (categoryCanvas) {

        new Chart(categoryCanvas, {
            type: "doughnut",

            data: {
                labels: [
                    "Meyve & Sebze",
                    "İçecekler",
                    "Atıştırmalık",
                    "Diğer"
                ],

                datasets: [{
                    data: [38, 27, 21, 14],

                    backgroundColor: [
                        "#347c3a",
                        "#72b578",
                        "#b4d9b7",
                        "#e0eee1"
                    ],

                    borderWidth: 0,
                    hoverOffset: 5
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "76%",

                plugins: {
                    legend: {
                        display: false
                    },

                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return ` ${context.label}: %${context.raw}`;
                            }
                        }
                    }
                }
            }
        });
    }
}

document.addEventListener(
    "DOMContentLoaded",
    initializeDashboardCharts
);
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


const categorySearch = document.getElementById("categorySearch");

if (categorySearch) {

    categorySearch.addEventListener("input", function () {

        const searchValue = this.value
            .toLocaleLowerCase("tr-TR")
            .trim();

        document
            .querySelectorAll("#categoryTable tbody tr")
            .forEach(row => {

                const categoryName = row
                    .querySelector(".category-table-name strong")
                    ?.textContent
                    .toLocaleLowerCase("tr-TR") ?? "";

                row.style.display =
                    categoryName.includes(searchValue)
                        ? ""
                        : "none";
            });
    });
}


const productImageUrl =
    document.getElementById("productImageUrl");

const productPreviewImage =
    document.getElementById("productPreviewImage");

const imagePreviewPlaceholder =
    document.querySelector(".image-preview-placeholder");

if (productImageUrl && productPreviewImage) {

    function updateProductPreview() {

        const imageUrl = productImageUrl.value.trim();

        if (!imageUrl) {
            productPreviewImage.style.display = "none";

            if (imagePreviewPlaceholder) {
                imagePreviewPlaceholder.style.display = "flex";
            }

            return;
        }

        productPreviewImage.src = imageUrl;
    }

    productImageUrl.addEventListener(
        "input",
        updateProductPreview);

    productPreviewImage.addEventListener(
        "load",
        function () {

            this.style.display = "block";

            if (imagePreviewPlaceholder) {
                imagePreviewPlaceholder.style.display = "none";
            }
        });

    productPreviewImage.addEventListener(
        "error",
        function () {

            this.style.display = "none";

            if (imagePreviewPlaceholder) {
                imagePreviewPlaceholder.style.display = "flex";
            }
        });

    updateProductPreview();
}


const productSearch = document.getElementById("productSearch");

if (productSearch) {

    productSearch.addEventListener("input", function () {

        const searchValue = this.value
            .toLocaleLowerCase("tr-TR")
            .trim();

        document
            .querySelectorAll("#productTable tbody tr")
            .forEach(row => {

                const productName =
                    row.querySelector(".product-table-text strong")
                        ?.textContent
                        .toLocaleLowerCase("tr-TR") ?? "";

                const categoryName =
                    row.querySelector(".product-category-badge")
                        ?.textContent
                        .toLocaleLowerCase("tr-TR") ?? "";

                const isMatch =
                    productName.includes(searchValue) ||
                    categoryName.includes(searchValue);

                row.style.display = isMatch ? "" : "none";
            });
    });
}

document
    .querySelectorAll(".delete-product-form")
    .forEach(form => {

        form.addEventListener("submit", function (event) {

            const productName =
                this.dataset.product ?? "bu ürün";

            const confirmed = confirm(
                `"${productName}" ürününü silmek istediğinize emin misiniz?`
            );

            if (!confirmed) {
                event.preventDefault();
            }
        });
    });
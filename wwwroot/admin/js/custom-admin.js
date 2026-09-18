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

    const salesCanvas =
        document.getElementById("salesChart");

    const categoryCanvas =
        document.getElementById("categoryChart");

    const salesDataElement =
        document.getElementById("dashboardSalesData");

    const categoryDataElement =
        document.getElementById("dashboardCategoryData");


    // SALES CHART
    if (salesCanvas && salesDataElement) {

        const salesData =
            JSON.parse(salesDataElement.textContent);

        const labels =
            salesData.map(item => item.label);

        const sales =
            salesData.map(item => item.sales);

        const revenue =
            salesData.map(item => item.revenue);


        new Chart(salesCanvas, {
            type: "line",

            data: {
                labels: labels,

                datasets: [
                    {
                        label: "Satış",
                        data: sales,
                        borderColor: "#347c3a",
                        backgroundColor:
                            "rgba(52, 124, 58, 0.08)",
                        fill: true,
                        tension: 0.4,
                        borderWidth: 2,
                        pointRadius: 3,
                        pointHoverRadius: 5,
                        yAxisID: "y"
                    },
                    {
                        label: "Gelir",
                        data: revenue,
                        borderColor: "#9acb9e",
                        backgroundColor: "transparent",
                        tension: 0.4,
                        borderWidth: 2,
                        borderDash: [5, 5],
                        pointRadius: 3,
                        pointHoverRadius: 5,
                        yAxisID: "yRevenue"
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

                        callbacks: {
                            label: function (context) {

                                if (
                                    context.dataset.label ===
                                    "Gelir"
                                ) {
                                    const value =
                                        Number(context.raw);

                                    return (
                                        " Gelir: " +
                                        value.toLocaleString(
                                            "tr-TR",
                                            {
                                                style: "currency",
                                                currency: "TRY"
                                            }
                                        )
                                    );
                                }

                                return (
                                    " Satış: " +
                                    context.raw +
                                    " adet"
                                );
                            }
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
                        position: "left",

                        border: {
                            display: false
                        },

                        grid: {
                            color: "#eef1ef"
                        },

                        ticks: {
                            color: "#9aa39d",
                            precision: 0,

                            font: {
                                size: 8
                            }
                        }
                    },

                    yRevenue: {
                        beginAtZero: true,
                        position: "right",

                        border: {
                            display: false
                        },

                        grid: {
                            drawOnChartArea: false
                        },

                        ticks: {
                            color: "#9aa39d",

                            callback: function (value) {
                                return (
                                    "₺" +
                                    Number(value)
                                        .toLocaleString("tr-TR")
                                );
                            },

                            font: {
                                size: 8
                            }
                        }
                    }
                }
            }
        });
    }


    // CATEGORY CHART
    if (categoryCanvas && categoryDataElement) {

        const categoryData =
            JSON.parse(categoryDataElement.textContent);

        const categoryLabels =
            categoryData.map(item => item.label);

        const categoryValues =
            categoryData.map(item => item.value);

        const categoryColors = [
            "#347c3a",
            "#72b578",
            "#b4d9b7",
            "#e0eee1",
            "#9fc9a3",
            "#cce3ce",
            "#5f9d65",
            "#dbeadc"
        ];


        new Chart(categoryCanvas, {
            type: "doughnut",

            data: {
                labels: categoryLabels,

                datasets: [{
                    data: categoryValues,

                    backgroundColor:
                        categoryLabels.map(
                            (_, index) =>
                                categoryColors[
                                index %
                                categoryColors.length
                                ]
                        ),

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

                                const total =
                                    context.dataset.data.reduce(
                                        (sum, value) =>
                                            sum + Number(value),
                                        0
                                    );

                                const value =
                                    Number(context.raw);

                                const percentage =
                                    total > 0
                                        ? (
                                            value /
                                            total *
                                            100
                                        ).toFixed(1)
                                        : 0;

                                return (
                                    ` ${context.label}: ` +
                                    `${value} satış ` +
                                    `(%${percentage})`
                                );
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


const featureSearch = document.getElementById("featureSearch");

if (featureSearch) {
    featureSearch.addEventListener("input", function () {

        const searchValue = this.value
            .toLocaleLowerCase("tr-TR")
            .trim();

        document
            .querySelectorAll("#featureTable tbody tr")
            .forEach(row => {

                const title =
                    row.querySelector(".feature-table-text strong")
                        ?.textContent
                        .toLocaleLowerCase("tr-TR") ?? "";

                row.style.display =
                    title.includes(searchValue) ? "" : "none";
            });
    });
}


const featureImageUrl =
    document.getElementById("featureImageUrl");

const featurePreviewImage =
    document.getElementById("featurePreviewImage");

const featurePreviewPlaceholder =
    document.querySelector(".feature-preview-placeholder");

if (featureImageUrl && featurePreviewImage) {

    function updateFeaturePreview() {

        const imageUrl = featureImageUrl.value.trim();

        if (!imageUrl) {
            featurePreviewImage.style.display = "none";

            if (featurePreviewPlaceholder) {
                featurePreviewPlaceholder.style.display = "flex";
            }

            return;
        }

        featurePreviewImage.src = imageUrl;
    }

    featurePreviewImage.addEventListener("load", function () {

        this.style.display = "block";

        if (featurePreviewPlaceholder) {
            featurePreviewPlaceholder.style.display = "none";
        }
    });

    featurePreviewImage.addEventListener("error", function () {

        this.style.display = "none";

        if (featurePreviewPlaceholder) {
            featurePreviewPlaceholder.style.display = "flex";
        }
    });

    featureImageUrl.addEventListener(
        "input",
        updateFeaturePreview);

    updateFeaturePreview();
}


document
    .querySelectorAll(".delete-feature-form")
    .forEach(form => {

        form.addEventListener("submit", function (event) {

            const featureName =
                this.dataset.feature ?? "bu slider";

            const confirmed = confirm(
                `"${featureName}" sliderını silmek istediğinize emin misiniz?`
            );

            if (!confirmed) {
                event.preventDefault();
            }
        });
    });

const discountSearch =
    document.getElementById("discountSearch");

if (discountSearch) {

    discountSearch.addEventListener("input", function () {

        const searchValue = this.value
            .toLocaleLowerCase("tr-TR")
            .trim();

        document
            .querySelectorAll("#discountTable tbody tr")
            .forEach(row => {

                const title =
                    row.querySelector(".feature-table-text strong")
                        ?.textContent
                        .toLocaleLowerCase("tr-TR") ?? "";

                const product =
                    row.querySelector(".discount-product-name")
                        ?.textContent
                        .toLocaleLowerCase("tr-TR") ?? "";

                row.style.display =
                    title.includes(searchValue) ||
                        product.includes(searchValue)
                        ? ""
                        : "none";
            });
    });
}


const discountImageUrl =
    document.getElementById("discountImageUrl");

const discountPreviewImage =
    document.getElementById("discountPreviewImage");

const discountPreviewPlaceholder =
    document.querySelector(".discount-preview-placeholder");

if (discountImageUrl && discountPreviewImage) {

    function updateDiscountPreview() {

        const imageUrl =
            discountImageUrl.value.trim();

        if (!imageUrl) {

            discountPreviewImage.style.display = "none";

            if (discountPreviewPlaceholder) {
                discountPreviewPlaceholder.style.display = "flex";
            }

            return;
        }

        discountPreviewImage.src = imageUrl;
    }

    discountPreviewImage.addEventListener(
        "load",
        function () {

            this.style.display = "block";

            if (discountPreviewPlaceholder) {
                discountPreviewPlaceholder.style.display = "none";
            }
        });

    discountPreviewImage.addEventListener(
        "error",
        function () {

            this.style.display = "none";

            if (discountPreviewPlaceholder) {
                discountPreviewPlaceholder.style.display = "flex";
            }
        });

    discountImageUrl.addEventListener(
        "input",
        updateDiscountPreview);

    updateDiscountPreview();
}


document
    .querySelectorAll(".delete-discount-form")
    .forEach(form => {

        form.addEventListener("submit", function (event) {

            const discountName =
                this.dataset.discount ?? "bu indirimi";

            const confirmed = confirm(
                `"${discountName}" kampanyasını silmek istediğinize emin misiniz?`
            );

            if (!confirmed) {
                event.preventDefault();
            }
        });
    });

const saleQuantity =
    document.getElementById("Quantity");

const saleUnitPrice =
    document.getElementById("saleUnitPrice");

const saleTotalPreview =
    document.getElementById("saleTotalPreview");

function calculateSaleTotal() {

    if (!saleQuantity ||
        !saleUnitPrice ||
        !saleTotalPreview) {
        return;
    }

    const quantity =
        parseInt(saleQuantity.value) || 0;

    const unitPrice =
        parseFloat(
            saleUnitPrice.value.replace(",", ".")
        ) || 0;

    const total =
        quantity * unitPrice;

    saleTotalPreview.textContent =
        total.toLocaleString(
            "tr-TR",
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }
        ) + " ₺";
}

if (saleQuantity && saleUnitPrice) {

    saleQuantity.addEventListener(
        "input",
        calculateSaleTotal);

    saleUnitPrice.addEventListener(
        "input",
        calculateSaleTotal);

    calculateSaleTotal();
}


const saleSearch =
    document.getElementById("saleSearch");

if (saleSearch) {

    saleSearch.addEventListener("input", function () {

        const searchValue =
            this.value
                .toLocaleLowerCase("tr-TR")
                .trim();

        document
            .querySelectorAll("#saleTable tbody tr")
            .forEach(row => {

                const productName =
                    row.querySelector(".sale-product-name")
                        ?.textContent
                        .toLocaleLowerCase("tr-TR") ?? "";

                row.style.display =
                    productName.includes(searchValue)
                        ? ""
                        : "none";
            });
    });
}


document
    .querySelectorAll(".delete-sale-form")
    .forEach(form => {

        form.addEventListener(
            "submit",
            function (event) {

                const productName =
                    this.dataset.sale ?? "bu satış";

                const confirmed = confirm(
                    `"${productName}" satış kaydını silmek istediğinize emin misiniz?`
                );

                if (!confirmed) {
                    event.preventDefault();
                }
            });
    });


const subscriberSearch =
    document.getElementById("subscriberSearch");

if (subscriberSearch) {

    subscriberSearch.addEventListener("input", function () {

        const searchValue =
            this.value
                .toLocaleLowerCase("tr-TR")
                .trim();

        document
            .querySelectorAll("#subscriberTable tbody tr")
            .forEach(row => {

                const name =
                    row.querySelector(".subscriber-name")
                        ?.textContent
                        .toLocaleLowerCase("tr-TR") ?? "";

                const email =
                    row.querySelector(".subscriber-email")
                        ?.textContent
                        .toLocaleLowerCase("tr-TR") ?? "";

                row.style.display =
                    name.includes(searchValue) ||
                        email.includes(searchValue)
                        ? ""
                        : "none";
            });

    });
}


document
    .querySelectorAll(".delete-subscriber-form")
    .forEach(form => {

        form.addEventListener("submit", function (event) {

            const subscriber =
                this.dataset.subscriber ?? "bu abone";

            const confirmed = confirm(
                `"${subscriber}" aboneliğini silmek istediğinize emin misiniz?`
            );

            if (!confirmed) {
                event.preventDefault();
            }

        });

    });
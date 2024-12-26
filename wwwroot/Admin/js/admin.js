(function ($) {
    "use strict";

    // Spinner
    var spinner = function () {
        setTimeout(function () {
            if ($('#spinner').length > 0) {
                $('#spinner').removeClass('show');
            }
        }, 1);
    };
    spinner();


    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 1500, 'easeInOutExpo');
        return false;
    });


    // Sidebar Toggler
    $('.sidebar-toggler').click(function () {
        $('.sidebar, .content').toggleClass("open");
        return false;
    });


    // Progress Bar


    // Calender
    $('#calender').datetimepicker({
        inline: true,
        format: 'L'
    });


    // Testimonials carousel
    $(".testimonial-carousel").owlCarousel({
        autoplay: true,
        smartSpeed: 1000,
        items: 1,
        dots: true,
        loop: true,
        nav: false
    });


    // Worldwide Sales Chart
    var ctx1 = $("#worldwide-sales").get(0).getContext("2d");
    var myChart1 = new Chart(ctx1, {
        type: "bar",
        data: {
            labels: ["2016", "2017", "2018", "2019", "2020", "2021", "2022"],
            datasets: [{
                label: "USA",
                data: [15, 30, 55, 65, 60, 80, 95],
                backgroundColor: "rgba(0, 156, 255, .7)"
            },
            {
                label: "UK",
                data: [8, 35, 40, 60, 70, 55, 75],
                backgroundColor: "rgba(0, 156, 255, .5)"
            },
            {
                label: "AU",
                data: [12, 25, 45, 55, 65, 70, 60],
                backgroundColor: "rgba(0, 156, 255, .3)"
            }
            ]
        },
        options: {
            responsive: true
        }
    });


    // Salse & Revenue Chart
    var ctx2 = $("#salse-revenue").get(0).getContext("2d");
    var myChart2 = new Chart(ctx2, {
        type: "line",
        data: {
            labels: ["2016", "2017", "2018", "2019", "2020", "2021", "2022"],
            datasets: [{
                label: "Salse",
                data: [15, 30, 55, 45, 70, 65, 85],
                backgroundColor: "rgba(0, 156, 255, .5)",
                fill: true
            },
            {
                label: "Revenue",
                data: [99, 135, 170, 130, 190, 180, 270],
                backgroundColor: "rgba(0, 156, 255, .3)",
                fill: true
            }
            ]
        },
        options: {
            responsive: true
        }
    });



    // Single Line Chart
    var ctx3 = $("#line-chart").get(0).getContext("2d");
    var myChart3 = new Chart(ctx3, {
        type: "line",
        data: {
            labels: [50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150],
            datasets: [{
                label: "Salse",
                fill: false,
                backgroundColor: "rgba(0, 156, 255, .3)",
                data: [7, 8, 8, 9, 9, 9, 10, 11, 14, 14, 15]
            }]
        },
        options: {
            responsive: true
        }
    });


    // Single Bar Chart
    var ctx4 = $("#bar-chart").get(0).getContext("2d");
    var myChart4 = new Chart(ctx4, {
        type: "bar",
        data: {
            labels: ["Italy", "France", "Spain", "USA", "Argentina"],
            datasets: [{
                backgroundColor: [
                    "rgba(0, 156, 255, .7)",
                    "rgba(0, 156, 255, .6)",
                    "rgba(0, 156, 255, .5)",
                    "rgba(0, 156, 255, .4)",
                    "rgba(0, 156, 255, .3)"
                ],
                data: [55, 49, 44, 24, 15]
            }]
        },
        options: {
            responsive: true
        }
    });


    // Pie Chart
    var ctx5 = $("#pie-chart").get(0).getContext("2d");
    var myChart5 = new Chart(ctx5, {
        type: "pie",
        data: {
            labels: ["Italy", "France", "Spain", "USA", "Argentina"],
            datasets: [{
                backgroundColor: [
                    "rgba(0, 156, 255, .7)",
                    "rgba(0, 156, 255, .6)",
                    "rgba(0, 156, 255, .5)",
                    "rgba(0, 156, 255, .4)",
                    "rgba(0, 156, 255, .3)"
                ],
                data: [55, 49, 44, 24, 15]
            }]
        },
        options: {
            responsive: true
        }
    });


    // Doughnut Chart
    var ctx6 = $("#doughnut-chart").get(0).getContext("2d");
    var myChart6 = new Chart(ctx6, {
        type: "doughnut",
        data: {
            labels: ["Italy", "France", "Spain", "USA", "Argentina"],
            datasets: [{
                backgroundColor: [
                    "rgba(0, 156, 255, .7)",
                    "rgba(0, 156, 255, .6)",
                    "rgba(0, 156, 255, .5)",
                    "rgba(0, 156, 255, .4)",
                    "rgba(0, 156, 255, .3)"
                ],
                data: [55, 49, 44, 24, 15]
            }]
        },
        options: {
            responsive: true
        }
    });


})(jQuery);




document.addEventListener("DOMContentLoaded", function () {
    const chipContainer = document.querySelector(".chips_input .inner");
    const inputField = document.getElementById("myinput");
    const chipLimit = parseInt(document.querySelector(".chips_input").getAttribute("data-limit"), 10);
    const chipAddition = '<button aria-label="remove this chip"><i class="bi bi-x-circle"></i></button>';
    const chipList = [];  // Array to store chip values
    const locationInput = document.getElementById("location");

    updateLimiter();

    // Focus on input when clicking container
    chipContainer.addEventListener("click", () => {
        inputField.focus();
    });

    // Handle keydown events for Enter, Tab, and comma
    inputField.addEventListener("keydown", (e) => {
        if (["Enter", "Tab", ","].includes(e.key)) {
            e.preventDefault();
            let value = inputField.value.trim();

            // Remove trailing comma if it's the key pressed
            if (e.key === ",") {
                value = value.slice(0, -1);
            }

            // Check if value is valid, unique, and within limit
            if (value && !hasDuplicateChip(value) && getChipCount() < chipLimit) {
                makeChip(value);
                inputField.value = "";
                updateLocationInput(); // Update hidden input
                document.getElementById('locationSearchForm').submit();
            }
        } else if (e.key === "Backspace" && inputField.value.length === 0) {
            removeLastChip();
            updateLimiter();
        }
    });

    // Handle removing chips on button click
    chipContainer.addEventListener("click", (e) => {
        if (e.target.closest("button")) {
            const chipElement = e.target.closest(".chip");
            const chipValue = chipElement.textContent.trim(); // Remove the "remove" button text
            chipElement.remove();
            updateChipList(chipValue, false); // Remove from chip list

            updateLimiter();
            updateLocationInput(); // Update hidden input

            document.getElementById('locationSearchForm').submit();
        }
    });

    // Create a new chip
    function makeChip(text) {
        const chip = document.createElement("span");
        chip.classList.add("chip");
        chip.innerHTML = `${escapeHtml(text)}${chipAddition}`;
        chipContainer.insertBefore(chip, inputField);
        updateChipList(text, true); // Add to chip list
        updateLimiter();
    }

    // Count the chips
    function getChipCount() {
        return chipContainer.querySelectorAll(".chip").length;
    }

    // Check for duplicate chips
    function hasDuplicateChip(value) {
        const chips = chipContainer.querySelectorAll(".chip");
        return Array.from(chips).some(chip => chip.textContent.trim() === value.trim());
    }

    // Remove the last chip
    function removeLastChip() {
        const lastChip = chipContainer.querySelector(".chip:last-of-type");
        if (lastChip) {
            const chipValue = lastChip.textContent.trim().slice(0, -1); // Get chip value without remove button
            lastChip.remove();
            updateChipList(chipValue, false); // Remove from chip list
        }
    }

    // Update chip list in the chipList array (adding/removing)
    function updateChipList(chipValue, isAdding) {
        // Trim the chip value to avoid issues with leading/trailing spaces


        let chipIndex = -1;  // Default to -1 (not found)

        // Use forEach to iterate and find the chip's index
        chipList.forEach((chip, index) => {
            console.log('chipValue >>', chipValue, 'chip >>', chip,);

            if (chip.trim() == chipValue) {

                chipIndex = index;  // Set the index if found
            }
        });

        // If adding and chip is not already in the list
        if (isAdding && chipIndex === -1) {
            chipList.push(chipValue); // Add chip to list
        }

        // If removing and chip is found in the list
        if (!isAdding && chipIndex !== -1) {
            chipList.splice(chipIndex, 1); // Remove chip from list
        }
        console.log(chipList);

        // Update the hidden location input field with the current chip list
    }
    function updateLocationInput() {
        console.log(chipList)
        locationInput.value = chipList.join(",");  // Set the value as a comma-separated list of chips
    }

    // Update chip limit display
    function updateLimiter() {
        const limitLabel = document.querySelector(".chips_input > label > .limit");
        const currentCount = getChipCount();
        const remaining = chipLimit - currentCount;

        if (limitLabel) {
            limitLabel.textContent = `(${remaining}/${chipLimit})`;
            limitLabel.style.color = remaining === 0 ? "var(--invalid)" : "inherit";
        }
    }



    // Escape HTML to prevent injection
    function escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    // Populate chips from SelectedLocation after page reload
    const selectedLocation = "@Model.SelectedLocation";
    if (selectedLocation) {
        const locations = selectedLocation.split(",").map(location => location.trim());
        locations.forEach(location => {
            makeChip(location);
        });
    }


});

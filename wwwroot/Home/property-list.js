  
  // Function to update the slider colors
    function updateProgressBars() {
      const priceRangeInput = document.querySelectorAll(".range-input input"),
    priceRangeProgress = document.querySelector(".slider .progress");
    const spaceRangeInput = document.querySelectorAll(".range-input-space input"),
    spaceRangeProgress = document.querySelector(".slider-space .progress-space");

    // Price Range Progress
    let minPrice = parseInt(priceRangeInput[0].value),
    maxPrice = parseInt(priceRangeInput[1].value);
    priceRangeProgress.style.left = (minPrice / priceRangeInput[0].max) * 100 + "%";
    priceRangeProgress.style.right = 100 - (maxPrice / priceRangeInput[1].max) * 100 + "%";

    // Space Range Progress
    let minSpace = parseInt(spaceRangeInput[0].value),
    maxSpace = parseInt(spaceRangeInput[1].value);
    spaceRangeProgress.style.left = (minSpace / spaceRangeInput[0].max) * 100 + "%";
    spaceRangeProgress.style.right = 100 - (maxSpace / spaceRangeInput[1].max) * 100 + "%";
    }

    // Initialize the progress bars on page load
    window.onload = updateProgressBars;

    // Price Range Logic
    const rangeInput = document.querySelectorAll(".range-input input"),
    priceInput = document.querySelectorAll(".price-input input"),
    range = document.querySelector(".slider .progress");
    let priceGap = 1000;

    priceInput.forEach((input) => {
        input.addEventListener("input", (e) => {
            let minPrice = parseInt(priceInput[0].value),
                maxPrice = parseInt(priceInput[1].value);

            if (maxPrice - minPrice >= priceGap && maxPrice <= rangeInput[1].max) {
                if (e.target.className === "input-min") {
                    rangeInput[0].value = minPrice;
                    range.style.left = (minPrice / rangeInput[0].max) * 100 + "%";
                } else {
                    rangeInput[1].value = maxPrice;
                    range.style.right = 100 - (maxPrice / rangeInput[1].max) * 100 + "%";
                }
            }
            updateProgressBars(); // Update the progress bar
        });
    });

    rangeInput.forEach((input) => {
        input.addEventListener("input", (e) => {
            let minVal = parseInt(rangeInput[0].value),
                maxVal = parseInt(rangeInput[1].value);

            if (maxVal - minVal < priceGap) {
                if (e.target.className === "range-min") {
                    rangeInput[0].value = maxVal - priceGap;
                } else if (e.target.className === "range-max") {
                    rangeInput[1].value = minVal + priceGap;
                }
            } else {
                priceInput[0].value = minVal;
                priceInput[1].value = maxVal;
                range.style.left = (minVal / rangeInput[0].max) * 100 + "%";
                range.style.right = 100 - (maxVal / rangeInput[1].max) * 100 + "%";
            }
            updateProgressBars(); // Update the progress bar
        });
    });

    // Space Range Logic
    const rangeInputSpace = document.querySelectorAll(".range-input-space input"),
    spaceInput = document.querySelectorAll(".space-input input"),
    rangeSpace = document.querySelector(".slider-space .progress-space");
    let spaceGap = 1; // Adjusted gap for space

    spaceInput.forEach((input) => {
        input.addEventListener("input", (e) => {
            let minSpace = parseInt(spaceInput[0].value),
                maxSpace = parseInt(spaceInput[1].value);

            if (maxSpace - minSpace >= spaceGap && maxSpace <= rangeInputSpace[1].max) {
                if (e.target.className === "input-min-space") {
                    rangeInputSpace[0].value = minSpace;
                    rangeSpace.style.left = (minSpace / rangeInputSpace[0].max) * 100 + "%";
                } else {
                    rangeInputSpace[1].value = maxSpace;
                    rangeSpace.style.right = 100 - (maxSpace / rangeInputSpace[1].max) * 100 + "%";
                }
            }
            updateProgressBars(); // Update the progress bar
        });
    });

    rangeInputSpace.forEach((input) => {
        input.addEventListener("input", (e) => {
            let minVal = parseInt(rangeInputSpace[0].value),
                maxVal = parseInt(rangeInputSpace[1].value);

            if (maxVal - minVal < spaceGap) {
                if (e.target.className === "range-min-space") {
                    rangeInputSpace[0].value = maxVal - spaceGap;
                } else if (e.target.className === "range-max-space") {
                    rangeInputSpace[1].value = minVal + spaceGap;
                }
            } else {
                spaceInput[0].value = minVal;
                spaceInput[1].value = maxVal;
                rangeSpace.style.left = (minVal / rangeInputSpace[0].max) * 100 + "%";
                rangeSpace.style.right = 100 - (maxVal / rangeInputSpace[1].max) * 100 + "%";
            }
            updateProgressBars(); // Update the progress bar
        });
    });


const minPriceInput = document.getElementById('min-price');
const maxPriceInput = document.getElementById('max-price');
const minRange = document.getElementById('min-range');
const maxRange = document.getElementById('max-range');

const minPriceInputForm = document.getElementById('min-price-form');
const maxPriceInputForm = document.getElementById('max-price-form');
const minRangeForm = document.getElementById('min-range-form');
const maxRangeForm = document.getElementById('max-range-form');

minPriceInput?.addEventListener('input', function () {
    minRange.value = minPriceInput.value;
    minRangeForm.value = minPriceInput.value;
});

maxPriceInput.addEventListener('input', function () {
    maxRange.value = maxPriceInput.value;
    maxRangeForm.value = maxPriceInput.value;
});

minRange.addEventListener('input', function () {
    minPriceInput.value = minRange.value;
    minPriceInputForm.value = minRange.value;
});

maxRange.addEventListener('input', function () {
    maxPriceInput.value = maxRange.value;
    maxPriceInputForm.value = maxRange.value;
});

// Space Range Synchronization
const minSpaceInput = document.getElementById('min-space-input');
const maxSpaceInput = document.getElementById('max-space-input');
const minSpaceRange = document.getElementById('min-space-range');
const maxSpaceRange = document.getElementById('max-space-range');


const minSpaceRangeForm = document.getElementById('min-space-form');
const maxSpaceRangeForm = document.getElementById('max-space-form');

minSpaceInput.addEventListener('input', function () {
    minSpaceRange.value = minSpaceInput.value;
    minSpaceRangeForm.value = minSpaceInput.value;
});


maxSpaceInput.addEventListener('input', function () {
    maxSpaceRange.value = maxSpaceInput.value;
    maxSpaceRangeForm.value = maxSpaceInput.value;
});

minSpaceRange.addEventListener('input', function () {
    minSpaceInput.value = minSpaceRange.value;
    minSpaceRangeForm.value = minSpaceRange.value;
});

maxSpaceRange.addEventListener('input', function () {
    maxSpaceInput.value = maxSpaceRange.value;
    maxSpaceRangeForm.value = maxSpaceRange.value;
});
maxSpaceRange.addEventListener('input', function () {
    maxSpaceInput.value = maxSpaceRange.value;
    maxSpaceRangeForm.value = maxSpaceRange.value;
});






// Property types object (keys are property types, values are their display names)
const propertyTypes = {
    Apartment: 'Apartment',
    Townhouse: 'Townhouse',
    VillaCompound: 'Villa Compound',
    ResidentialPlot: 'Residential Plot',
    ResidentialBuilding: 'Residential Building',
    Villa: 'Villa',
    HotelApartment: 'Hotel Apartment',
    ResidentialFloor: 'Residential Floor'
};

// Select the label and options container for Property Type
const labelType = document.querySelector('.dropdown__filter-selected-type');
const optionsContainerType = document.querySelector('.dropdown__select-type');
const valueType = document.getElementById('type-value');
// Generate dropdown options dynamically based on the `propertyTypes` object
for (const [key, value] of Object.entries(propertyTypes)) {
    const optionElementType = document.createElement('li');
    optionElementType.classList.add('dropdown__select-option');
    optionElementType.setAttribute('role', 'option');
    optionElementType.textContent = value;

    // Add click event to change the label when an option is selected
    optionElementType.addEventListener('click', () => {
        labelType.textContent = value; // Update label with selected value
        valueType.value = value;
    });

    // Append the option to the container
    optionsContainerType.appendChild(optionElementType);
}

// Close the dropdown when clicking outside
document.addEventListener('click', (e) => {
    const toggleType = document.querySelector('.dropdown__switch-type');
    const elementType = e.target;

    // If clicking on the toggle or an option, do nothing
    if (elementType === toggleType || elementType.closest('.dropdown__filter-type')) return;

    // Check if clicked outside the dropdown
    toggleType.checked = false;
});



// Example of finishing types object (You can add more types as needed)
const orderCase = {
    recent: 'Recent',
    minPrice: 'Min price',
    maxPrice: 'Max price',
    Oldest: 'Oldest'
};

// Select the label and options container for Finishing Type dropdown
const labelorderCase = document.querySelector('.dropdown__filter-selected-finishing');
const optionsContainerorderCase = document.querySelector('.dropdown__select-finishing');
const sort = document.querySelector('#sortInput');
// Generate dropdown options dynamically based on the `orderCase` object
for (const [key, value] of Object.entries(orderCase)) {
    const optionElementorderCase = document.createElement('li');
    optionElementorderCase.classList.add('dropdown__select-option');
    optionElementorderCase.setAttribute('role', 'option');
    optionElementorderCase.textContent = value;

    // Add click event to change the label when an option is selected
    optionElementorderCase.addEventListener('click', () => {
        labelorderCase.textContent = value; // Update label with selected value
        sort.value = value;
        document.getElementById('locationSearchForm').submit();

    });

    // Append the option to the container
    optionsContainerorderCase.appendChild(optionElementorderCase);
}

// Close the dropdown when clicking outside
document.addEventListener('click', (e) => {
    const toggleorderCase = document.querySelector('.dropdown__switch-finishing');
    const elementorderCase = e.target;

    // If clicking on the toggle or an option, do nothing
    if (elementorderCase === toggleorderCase || elementorderCase.closest('.dropdown__filter-finishing')) return;

    // Check if clicked outside the dropdown
    toggleorderCase.checked = false;
});





function submitForm() {
    document.getElementById('locationSearchForm').submit();
}






















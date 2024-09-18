function ConstructNewSelect(select, index) {
    // Create the dropdown container
    var dropdownDiv = document.createElement('div');
    dropdownDiv.id = 'dropdown-' + index;
    dropdownDiv.classList.add('dropdown', 'toggle');

    // Create the checkbox input
    var checkbox = document.createElement('input');
    checkbox.id = 'toggle-' + index; // Ensure unique ID
    checkbox.type = 'checkbox';

    // Create the label
    var label = document.createElement('label');
    label.htmlFor = 'toggle-' + index; // Match the ID of the checkbox
    label.textContent = select.options[select.selectedIndex].text; // Set the label to the selected option's text

    // Create the unordered list
    var ul = document.createElement('ul');

    // For each option in the select element, create a list item
    Array.from(select.options).forEach(function (option) {
        var li = document.createElement('li');
        var a = document.createElement('a');
        a.textContent = option.text; // Set the text of the link to the text of the option
        a.href = '#';
        li.appendChild(a);
        li.setAttribute('data-value', option.value); // Store the value in a data attribute

        // Update select and label on click
        li.addEventListener('click', function () {
            select.value = option.value; // Update the select value
            label.textContent = option.text; // Update the label
            checkbox.checked = false; // Close the dropdown
        });

        ul.appendChild(li); // Add the list item to the unordered list
    });

    // Hide the original select element
    select.style.display = 'none';

    // Build the dropdown
    dropdownDiv.appendChild(checkbox);
    dropdownDiv.appendChild(label);
    dropdownDiv.appendChild(ul);

    // Insert the new dropdown before the select element
    select.parentNode.insertBefore(dropdownDiv, select);
}

//function UpdateSelect(select) {
//    // Create the unordered list
//    var ul = document.createElement('ul');

//    // For each option in the select element, create a list item
//    Array.from(select.options).forEach(function (option) {
//        var li = document.createElement('li');
//        var a = document.createElement('a');

//        a.textContent = option.text; // Set the text of the link to the text of the option
//        a.href = '#';
//        li.appendChild(a);
//        li.setAttribute('data-value', option.value); // Store the value in a data attribute

//        // Update select and label on click
//        li.addEventListener('click', function () {
//            select.value = option.value; // Update the select value
//            label.textContent = option.text; // Update the label
//            checkbox.checked = false; // Close the dropdown
//        });

//        ul.appendChild(li); // Add the list item to the unordered list
//        //replace the ul of the old select with the new one

//    });
//    //replace the ul of the old select with the new one
//    select.parentNode.replaceChild(ul, select);

//}
document.addEventListener('DOMContentLoaded', function () {
    // Find all select elements with the class 'NewDropdown'
    var selects = document.querySelectorAll('select.NewDropdown');

    selects.forEach(function (select, index) {
        ConstructNewSelect(select, index);
    });
   
});
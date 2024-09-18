let selects = [];
function GetVersions( id,context){
    //make ajax request to get the Versions of the document

    $.ajax({
        type: "POST",
        url: "ArchivedDocuments/GetVersions",
        data: { doc_id: id },
        success: function (data) {
            //display the logs in the logs page wrapped in option tags
            var options = '';
            options='<option value="Latest">Latest</option>'
            data.forEach(function (data) {
                if (data != '-001') {
                    options += '<option value="' + data + '">' + data + '</option>'
                }
                
            });
            //get the select inside the context

            const selectElement = $(context).find('select').first();

            const elem = context.querySelector('select.NewDropdown');

            elem.innerHTML = options;
            UpdateSelect(elem,elem.id);

            

        }
    });

}


function UpdateSelect(select, index) {
    // get the dropdown container
    var dropdownDiv = document.getElementById('dropdown-' + index);

    // get the checkbox input
    var checkbox = document.getElementById('toggle-' + index);

    // get the label
    var label = dropdownDiv.querySelector('label');

    // get the ul
    var ul = dropdownDiv.querySelector('ul');
    ul.innerHTML = '';
    // get the selected option
    var selectedOption = select.options[select.selectedIndex];

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

}

document.addEventListener('DOMContentLoaded', function () {
    // Find all select elements with the class 'NewDropdown'
    selects = document.querySelectorAll('select.NewDropdown');

    selects.forEach(function (select, index) {
        select.addEventListener('change', function () {
            alert("change");
            UpdateSelect(select, index);
        });
    });

});
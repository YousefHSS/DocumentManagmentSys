
document.addEventListener('DOMContentLoaded', function () {
    const editor = document.getElementById('editor');

});

function AddStrengthInput() {
    // Assuming getNumberOfInstances() returns the current count of instances
    var numOfInstances = getNumberOfInstances();

    // Increment the number of instances to get a unique ID for the new editor
    numOfInstances++;

    var strength = document.getElementById("strengths");
    var newInputId = 'editor' + numOfInstances; // Create a unique ID for the new editor
    var newDiv = document.createElement('div');
    newDiv.className = "editable p-2 rounded-1 border-1 border m-1";
    newDiv.id = newInputId;
    newDiv.setAttribute('contenteditable', 'true');
    newDiv.innerHTML = '<p></p>';

    // Append the new div to the strengths container
    strength.appendChild(newDiv);

    // Initialize a new CKEditor instance on the newly added div
    CKEDITOR.inline(newInputId);


}


function getNumberOfInstances() {
    var x = 0; 
    for (var instances in CKEDITOR.instances) {
        x++;
    }
    return x;
}


function SetPage(page) {
    //submit form
    var form = document.getElementById("documentForm");
    form["page"].value = page;

    PrepareDataForSubmit(form);
    form.submit();
}

function SaveDocument() {
    var form = document.getElementById("SaveDocument");
    PrepareDataForSubmit(form)
    form.submit();
}

function DecodeCKEDITORData() {
    var stringArray ="";
    for (var i in CKEDITOR.instances)
    {
        stringArray += CKEDITOR.instances[i].getData();
        stringArray += "__SEP__";
    }
    return stringArray;
    
    
}

function PrepareDataForSubmit(form) {
    
    //get all ToBeJson elements
    var inputs = document.getElementsByClassName("ToBeJson");
    //format the input as an array seperated by __SEP__ as a string

    var stringArray = "";
    for (var i = 0; i < inputs.length; i++) {
        if (i != 0) {
            stringArray += "__SEP__";
        }
        stringArray += inputs[i].value;
    }
    //ToBeJson form element injected
    //create a new element
    var input = document.createElement("input");
    input.type = "hidden";
    input.name = "ToBeJson";
    //if decode is null then use stringArray
    input.value = DecodeCKEDITORData() == "" ? stringArray : DecodeCKEDITORData();
    form.appendChild(input);
}

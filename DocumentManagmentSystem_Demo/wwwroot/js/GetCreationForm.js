
document.addEventListener('DOMContentLoaded', function () {
    const editor = document.getElementById('editor');


    style_CK_block();



});


function style_CK_block() {
    /*get the editor instance that you want to interact with.*/
    //get all editor instances
    const editors = document.querySelectorAll('.editable');
    var NoNonEditable = true;
    for (var i = 0; i < editors.length; i++) {
        editors[i].style.backgroundColor = "gainsboro";
        //in each editor get each span element
        const spans = editors[i].querySelectorAll('span');
        for (var j = 0; j < spans.length; j++) {
            //if the first span has contenteditable="false" change padding left to 0 important and set important
            if (spans[j].getAttribute('contenteditable') == "true") {
                spans[j].classList.add("SpanEditable");
                //check next span and prev span to remove padding only on the current span
                if (j + 1 < spans.length && spans[j + 1].getAttribute('contenteditable') == "true") {
                    spans[j].style.paddingRight="0";
                }
                if(j-1 >= 0 && spans[j-1].getAttribute('contenteditable') == "true"){
                    spans[j].style.paddingLeft="0";
                }
               
            }
            else {
               
                NoNonEditable = false;
            }
            
          
        }

        if (NoNonEditable) {
            editors[i].style.backgroundColor = "white";
        }

    }
    //get elements that have the contenteditable attribute
    const toBeBlock = document.querySelectorAll('[contenteditable="false"]');
    for (var i = 0; i < toBeBlock.length; i++) {
        //add background color gainsboro
        //if it is not p or li
        if (toBeBlock[i].tagName != "P" && toBeBlock[i].tagName != "LI") {
            toBeBlock[i].style.backgroundColor = "gainsboro";
        }
    }
}

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
    newDiv.innerHTML = '<p><span></span></p>';

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

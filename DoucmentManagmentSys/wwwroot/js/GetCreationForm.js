
document.addEventListener('DOMContentLoaded', function () {
    const editor = document.getElementById('editor');

});

function AddStrengthInput() {
    //add to id strengths and a new input
    var strength = document.getElementById("strengths");
    var newInput = `@TextInputElement(new Run(new Text("")))`;
    strength.innerHTML += newInput;


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
    
    var input = document.createElement("input");
    input.type = "hidden";
    input.name = "ToBeJson";
    input.value = DecodeCKEDITORData();
    form.appendChild(input);
}

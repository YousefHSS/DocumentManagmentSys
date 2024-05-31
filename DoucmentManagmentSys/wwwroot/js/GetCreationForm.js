
//
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
    //get all ToBeJson elements
    var inputs = document.getElementsByClassName("ToBeJson");
    //format the input as an array seperated by , as a string

    var stringArray = "";
    for (var i = 0; i < inputs.length; i++) {
        if (i != 0) {
            stringArray += ",";
        }
        stringArray += inputs[i].value;
    }
    //ToBeJson form element injected
    //create a new element
    var input = document.createElement("input");
    input.type = "hidden";
    input.name = "ToBeJson";
    input.value = stringArray;
    form.appendChild(input);

    form.submit();
}

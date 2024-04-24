// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function copyHeaderWidths(sourceTable, targetTable) {
    const sourceHeaders = sourceTable.querySelectorAll('th');
    const targetHeaders = targetTable.querySelectorAll('th');
    var z = 0;
    if (sourceHeaders.length !== targetHeaders.length) {
        console.error('Tables have different number of headers');
        return;
    }

    for (let i = 0; i < sourceHeaders.length; i++) {
        const sourceWidth = window.getComputedStyle(sourceHeaders[i]).getPropertyValue('width');
        targetHeaders[i].style.width = sourceWidth;
        targetHeaders[i].style.zIndex = z++;
        
    }
}

//on document ready
document.addEventListener("DOMContentLoaded", function (event) {
    const sourceTable = document.getElementById('sourceTable');
    const targetTable = document.getElementById('targetTable');

    if (sourceTable && targetTable) {
        copyHeaderWidths(sourceTable, targetTable);

    }




   
});


function toggleTransition() {

    const element = document.getElementById('targetTable');

    const TR = document.getElementById('HH');
    //get computed hight
    
    element.classList.toggle('transitionTBShow');
    TR.classList.toggle('ShowHightTR');
    var toggle = element.classList.contains('tesTR');
    if (toggle) {


       
        
    }
    else {

        
    }

}


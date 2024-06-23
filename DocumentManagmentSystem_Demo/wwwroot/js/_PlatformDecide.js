import  BackgroundFloatAnimation  from './BackgroundFloatAnimation.js';
let screenWidth = window.innerWidth;
const right = document.querySelector(".right");
const left = document.querySelector(".left");
const container = document.querySelector(".container");
const rightAnimation = new BackgroundFloatAnimation("Right");
const leftAnimation = new BackgroundFloatAnimation("Left");

left.addEventListener("mouseenter", () => {
    container.classList.add("hover-left");
    leftAnimation.startAnimation();
    changeSizePD();
})

left.addEventListener("mouseleave", () => {
    container.classList.remove("hover-left");
    leftAnimation.pauseAnimation();
    changeSizePD();
})


right.addEventListener("mouseenter", () => {
    container.classList.add("hover-right");
    rightAnimation.startAnimation();
    changeSizePD();
})

right.addEventListener("mouseleave", () => {
    container.classList.remove("hover-right");
    rightAnimation.pauseAnimation();
    changeSizePD();
})

function changeSizePD() {
    screenWidth = window.innerWidth;
    document.documentElement.style.setProperty('--screenWidth', `${screenWidth}px`);

    if (container.classList.contains("hover-left")) {
        var bigger = screenWidth * 0.75;
        var smaller = screenWidth * 0.25;
        left.style.width = `${bigger}px`;
        right.style.width = `${smaller}px`;

    }

    else if (container.classList.contains("hover-right")) {
        var bigger = screenWidth * 0.75;
        var smaller = screenWidth * 0.25;
        right.style.width = `${bigger}px`;
        left.style.width = `${smaller}px`;

    }
    else {
        left.style.width = `${screenWidth * 0.5}px`;
        right.style.width = `${screenWidth * 0.5}px`;
    }
}

document.addEventListener("DOMContentLoaded", () => {
    let screenWidth = window.innerWidth;
    changeSizePD();

});

//on window size change
document.addEventListener("resize", () => {
    changeSizePD();
    alert("resize");
});





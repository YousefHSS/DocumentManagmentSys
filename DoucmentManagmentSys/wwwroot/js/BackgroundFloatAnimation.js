export default class BackgroundFloatAnimation {
    constructor(LefToRight = "Left") {
        this.animationDuration = 10000;
        this.LefToRight = LefToRight;
        this.startTime;
        this.progress = 0; // Progress is a percentage of the animation
        this.animationFrameId;
    }

     startAnimation() {

        
        // Set the start time based on the current progress
         this.startTime = Date.now() - this.progress * this.animationDuration;

        // Start or resume the animation loop
        this.runAnimation();
    }

     updateBackgroundPosition(newPosition) {
         document.documentElement.style.setProperty('--background-position-x' + this.LefToRight, `${newPosition}%`);
    }

     runAnimation() {
        const currentTime = Date.now();
        const elapsedTime = currentTime - this.startTime;
         this.progress = (elapsedTime / this.animationDuration) % 1;

         const backgroundPositionX = this.progress * 100; // Convert to percentage
         this.updateBackgroundPosition(backgroundPositionX, this.LefToRight);

         this.animationFrameId = requestAnimationFrame(() => this.runAnimation());
    }

    pauseAnimation() {
       
        cancelAnimationFrame(this.animationFrameId);
    }
}

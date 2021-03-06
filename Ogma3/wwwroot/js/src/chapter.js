new Vue({
   el: '#chapter-app',
   data: {
      // lastScroll: 0,
      ticking: false,
      
      windowHeight: 0,
      containerHeight: 0,
      progress: 0,
   },
   methods: {
      report: function () {
         this.$refs.reportModal.visible = true;
      },
      handleScroll: function () {
         // console.log(this.lastScroll)
         let elBottom = this.$el.getBoundingClientRect().bottom;
         let perc = elBottom - this.windowHeight;
         
         this.progress = 1 - perc.normalize(0, this.containerHeight - (this.windowHeight)).clamp();
      }
   },
   mounted() {
      this.windowHeight = window.innerHeight;
      this.containerHeight = this.$el.offsetTop + this.$el.offsetHeight;
      
      document.addEventListener('scroll', e => {
         // this.lastScroll = window.scrollY;
         if (!this.ticking) {
            window.requestAnimationFrame(() => {
               this.handleScroll();
               this.ticking = false;
            });
            this.ticking = true;
         }
      });
   }
});
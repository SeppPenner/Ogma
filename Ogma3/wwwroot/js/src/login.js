new Vue({ 
    el: "#app",
    data: {
        name: null,
        avatar: "key.webp",
        title: null,
        // hasMfa: false,
        checked: false,        
        
        cdn: null,
        route: null
    },
    methods: {
        checkDetails: function (e) {
            e.preventDefault();
            
            if (this.name) {
                axios.get(this.route + '/signin/' + this.name)
                    .catch(e => console.error(e))
                    .then(r => {
                        if(r.status === 200) {
                            let d = r.data;
                            this.avatar = d.avatar;
                            this.title = d.title;
                            // this.hasMfa = d.hasMfa; 
                        }
                        this.checked = true;
                    });
            }
        },
        reset: function (e) {
            this.avatar = null;
            this.title = null;
            // this.hasMfa = false;
            this.checked = false;
        }
    },
    
    computed: {
        getAvatar: function() {
            return this.cdn + (this.avatar ?? 'key.webp');
        }
    },
    
    mounted() {
        // Grab the route from route helper
        this.route = document.getElementById('route').dataset.route; 
        this.cdn = document.getElementById('cdn').dataset.cdn;
    }
});


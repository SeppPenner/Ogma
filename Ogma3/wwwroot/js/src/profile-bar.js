new Vue({
    el: '#profile-bar',
    data: {
        route: null,
        name: null,
        xcsrf: null,
    },
    methods: {
        follow: function () {
            
        },
        block: function () {
            axios.post(this.route + '/block',
                {
                    name: this.name
                }, 
                {
                    headers: {
                        'RequestVerificationToken': this.xcsrf
                    }
                }
            )
            .then(res => {
                console.log(res.data)
            })
            .catch(console.error)
        }
    },
    mounted() {
        this.route  = document.getElementById('data-route').dataset.route;
        this.name   = document.getElementById('data-name').dataset.name; 
        this.xcsrf  = document.querySelector('[name=__RequestVerificationToken]').value;
    }
});
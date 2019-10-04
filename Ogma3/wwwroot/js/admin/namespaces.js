let vue = new Vue({ 
    el: "#app",
    data: {
        form: {
            name: null,
            id: null
        },
        namespaces: [],
        route: null
    },
    methods: {

        // Contrary to its name, it also modifies a namespace if needed.
        // It was simply easier to slap both functionalities into a single function.
        createNamespace: function(e) {
            e.preventDefault();

            if (this.form.name) {

                // If no ID has been set, that means it's a new namespace.
                // Thus, we POST it.
                if (this.form.id === null) {
                    axios.post(this.route,
                        {
                            name: this.form.name,
                        })
                        .then(_ => {
                            this.getNamespaces()
                        })
                        .catch(error => {
                            console.log(error);
                        });
                    
                // If the ID is set, that means it's an existing namespace.
                // Thus, we PUT it.
                } else {
                    axios.put(this.route + '/' + this.form.id,
                        {
                            id: this.form.id,
                            name: this.form.name,
                        })
                        .then(_ => {
                            this.getNamespaces()
                        })
                        .catch(error => {
                            console.log(error);
                        })
                        // Clear the form too
                        .then(_ => {
                            this.form.name =
                                this.form.id = null;
                        });
                }

            }
        },

        // Gets all existing namespaces
        getNamespaces: function() {
            axios.get(this.route)
                .then(response => {
                    this.namespaces = response.data
                })
                .catch(error => {
                    console.log(error);
                });
        },

        // Deletes a selected namespace
        deleteNamespace: function(t) {
            axios.delete(this.route + '/' + t.id) 
                .then(_ => {
                    this.getNamespaces() 
                })
                .catch(error => {
                    console.log(error);
                });
        },

        // Throws a namespace from the list into the editor
        editNamespace: function(t) {
            this.form.name = t.name;
            this.form.id = t.id; 
        },

        // Clears the editor
        cancelEdit: function() {
            this.form.name =
                this.form.id = null;    
        }
    },
    
    mounted() {
        // Grab the route from route helper
        this.route = document.getElementById('route').dataset.route;
        // Grab the initial set of namespaces
        this.getNamespaces();
    }
});
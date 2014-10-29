(function () {
    // Get Swashbuckle config into JavaScript
    var sbConfig = {
        discoveryBaseUrl: %(DiscoveryBaseUrl),
        discoveryPaths: [ %(DiscoveryPaths) ]
    };

    var DiscoveryUrlSelector = Backbone.View.extend({
        render: function() {
            // Don't re-render on subsequent reloads
            var defaultVal = this.$el.val()
            if (this.$el.prop('tagName') != 'SELECT') {
                var select = $('<select id="input_baseUrl" name="baseUrl"></select>');
                select
                    .css('margin', '0')
                    .css('border', '1px solid gray')
                    .css('padding', '3px')
                    .css('width', '400px')
                    .css('font-size', '0.9em');

                _.each(this.options.discoveryUrls, function(url) {
                    var option = $('<option>' + url + '</option>');
                    select.append(option);
                })
                select.val(defaultVal);
                this.$el.replaceWith(select);
            }
            return this;
        }
    });

    new DiscoveryUrlSelector({
        el: $('#input_baseUrl'),
        discoveryUrls: _.map(sbConfig.discoveryPaths, function(path) {
            return sbConfig.discoveryBaseUrl + path;
        }) 
    }).render();
})();
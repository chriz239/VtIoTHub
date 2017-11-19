// A simple templating method for replacing placeholders enclosed in curly braces.
if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

$(function () {

    var hub = $.connection.ioTSignalRHub, // the generated client-side hub proxy
        $humidityTable = $('#humidityTable'),
        $humidityTableBody = $humidityTable.find('tbody'),
        rowTemplate = '<tr id="{Device}"><td>{Device}</td><td>{Humidity}</td></tr>';

    function formatHumidityInfo(info) {
        return $.extend(info, {
            Device: info.Device.split(' ').join('_'),
            Humidity: info.Humidity
        });
    }

    function addInfo (humidityInfo) {
        var info = formatHumidityInfo(humidityInfo)
        //console.log(info.Device.split(' ').join('_'))
        var obj = $humidityTableBody.find('#' + info.Device.split(' ').join('_'))
        if (obj.length) {
            //$humidityTableBody.find('tr[data-symbol=' + info.Device + ']').replaceWith(rowTemplate.supplant(info))
            $humidityTableBody.find('#' + info.Device).replaceWith(rowTemplate.supplant(info))
        } else {
            $humidityTableBody.append(rowTemplate.supplant(info));
        }

    }

    function init() {
        //hub.server.getAllHumidityInfos().done(addInfo);
        $humidityTableBody.empty();
    }

    hub.client.updateHumidity = addInfo

    // Add a client-side hub method that the server will call
    /*function (info) {
        var humidityInfo = formatHumidityInfo(info),
            $row = $(rowTemplate.supplant(humidityInfo));

        $humidityTableBody.find('tr[data-symbol=' + stock.Symbol + ']')
            .replaceWith($row);
    }
        */

    // Start the connection
    $.connection.hub.start().done(init);

});
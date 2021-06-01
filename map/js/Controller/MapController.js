function MapController() {
    var _ = this;
    var proxy = new DataProxy();
    var login = new Login();
    var map;
    var size = 170;
    var small_size = 100;
    var records;
    var locations = [];
    var infos = [];
    var current_location;
    var current_info;

    _.INIT = function () {
        _.proxy = proxy;
        _.proxy.parent = _;
        _.login = login;
        _.login.parent = _;
        _.showMap();
        _.login.INIT();
        //Test for AddDot AddDirection Clear function
        // AddDot(1, 144.946457, -37.840935, 0, 2);
        // AddDirection(1, 144.946457, -37.840935, 144.976457, -37.820935, 2);
        // ClearMarker(1);
    }

    _.showMap = function () {
        mapboxgl.accessToken = map_accessToken;
        console.log("showmap");
        map = new mapboxgl.Map({
            container: 'map', // container id
            style: 'mapbox://styles/yuhe0925/ckmyi9chk1i4t17mn3iiepo70',
            center: [144.946457, -37.840935], // starting position [lng, lat]
            zoom: 14, // starting zoom
            attributionControl: false
        });

        map.on('load', function () {
            map.addControl(new mapboxgl.NavigationControl());
            // map.addImage('staticDot', staticDot, { pixelRatio: 2 });
            map.addImage('pulsing-dot', pulsingDot, { pixelRatio: 2 });

        });
        _.showInfo();
    }

    _.readData = function (package) {
        locations = [];
        infos = [];
        current_location = null;
        current_info = null;
        _.removeLayers();
        firstEnter = false;
        // console.log(package);
        if (package.trackingRecords != null) {
            records = package.trackingRecords;
            if (records.length > 30) {
                records = records.slice(-30);
            }
            for (record of records) {
                locations.push([record.gnssLongitude, record.gnssLatitude]);
                infos.push([record.attachedDevice, record.packageID, record.updated, record.gnssLongitude, record.gnssLatitude])
            }
            current_location = locations[locations.length - 1];
            current_info = infos[infos.length - 1];

        }
        _.draw();


    }

    _.removeLayers = function () {
        //remove exist layers
        for (var i = 0; i < locations.length - 1; i++) {
            if (map.getLayer('route' + i)) {
                map.removeLayer('route' + i);
            }
            if (map.getSource('route' + i)) {
                map.removeSource('route' + i);
            }
            if (map.getLayer('driveRoute' + i)) {
                map.removeLayer('driveRoute' + i);
            }
            if (map.getSource('driveRoute' + i)) {
                map.removeSource('driveRoute' + i);
            }
        }
        for (var j = 0; j < locations.length; j++) {

            if (map.getLayer('point' + j)) {
                map.removeLayer('point' + j);
            }
            if (map.getSource('point' + j)) {
                map.removeSource('point' + j);
            }
        }
        if (map.getLayer('current_point')) {
            map.removeLayer('current_point');
        }
        if (map.getSource('current_point')) {
            map.removeSource('current_point');
        }
    }

    _.drawStraightLines = function () {
        for (var i = 0; i < locations.length - 1; i++) {
            var line = [locations[i], locations[i + 1]];
            map.addSource('route' + i, {
                type: 'geojson',
                data: {
                    "type": "Feature",
                    "properties": {},
                    "geometry": {
                        "type": "LineString",
                        "coordinates": line
                    }
                }
            });

            map.addLayer({
                "id": 'route' + i,
                "type": "line",
                "source": 'route' + i,
                "layout": {
                    "line-join": "round",
                    "line-cap": "round"
                },
                "paint": {
                    "line-color": "#68B1FA",
                    "line-width": 8
                }
            });
        }
    }

    _.drawPoints = function () {
        for (var j = 0; j < infos.length; j++) {
            var date = _.ticksToDateString(infos[j][2]);
            map.addSource('point' + j, {
                type: 'geojson',
                data: {
                    "type": "Feature",
                    "properties": {
                        "description": "<Strong>Device:" + infos[j][0] + "</br>Package:" + infos[j][1] + "</Strong></br><Strong>Time:"
                            + date.substring(0, 10) + " " + date.substring(11, 19) + "</Strong></br><Strong>Location:[" + infos[j][3] + "," + infos[j][4] + "]</Strong><img class=\"carPic\"src=\"resource/car2.jpg\">",
                    },
                    "geometry": {
                        "type": "Point",
                        "coordinates": [infos[j][3], infos[j][4]]
                    }
                }
            });

            map.addLayer({
                "id": 'point' + j,
                "type": "circle",
                "source": 'point' + j,
                'paint': {
                    'circle-radius': 8,
                    'circle-color': 'White'
                }
            });
        }
    }

    _.drawCurrentPoints = function () {

        var date = _.ticksToDateString(current_info[2]);
        map.addSource("current_point", {
            type: 'geojson',
            data: {
                "type": "Feature",
                "properties": {
                    "description": "<Strong>Device:" + current_info[0] + "</br>Package:" + current_info[1] + "</Strong></br><Strong>Time:"
                        + date.substring(0, 10) + " " + date.substring(11, 19) + "</Strong></br><Strong>Location:[" + current_info[3] + "," + current_info[4] + "]</Strong><img class=\"carPic\"src=\"resource/car4.jpg\">",
                },
                "geometry": {
                    "type": "Point",
                    "coordinates": current_location
                }
            }
        });
        map.addLayer({
            "id": "current_point",
            // "type": "symbol",
            "type": "circle",
            "source": "current_point",
            // "layout": {
            //     "icon-image": "pulsing-dot"
            // }
            'paint': {
                'circle-radius': 8,
                'circle-color': 'Red'
            }
        });
        console.log("print red dot ...............");
    }


    _.draw = function () {
        //draw straight lines
        _.drawStraightLines();

        //drive road 
        for (var i = 0; i < locations.length - 1; i++) {
            _.DriveRoute(locations[i], locations[i + 1], i);
        }

        //past dots
        _.drawPoints();

        //current dot
        _.drawCurrentPoints();
        //move to the latest location view
        map.flyTo({ center: current_location });

    }

    _.DriveRoute = function (start, end, i) {
        var startEnd = encodeURIComponent(start + ';' + end);
        var route = _.proxy.OnRequestDirections(startEnd)[0];
        var driveCoordinates = route.geometry;
        _.drawDriveRoute(driveCoordinates, i);
    }
    _.drawDriveRoute = function (driveCoordinates, i) {
        map.addSource('driveRoute' + i, {
            type: 'geojson',
            data: driveCoordinates
        });
        map.addLayer({
            "id": "driveRoute" + i,
            'type': 'line',
            "source": "driveRoute" + i,
            'layout': {
                'line-join': 'round',
                'line-cap': 'round'
            },
            'paint': {
                'line-color': '#fbb03b',
                'line-width': 4,
                'line-opacity': 1
            }
        });
    }

    _.showInfo = function () {
        map.on('click', 'current_point', function (e) {
            var coordinates = e.features[0].geometry.coordinates.slice();
            var description = e.features[0].properties.description;
            //prevent the dots behind the current point show their infos
            e.preventDefault();
            // Ensure that if the map is zoomed out such that multiple
            // copies of the feature are visible, the popup appears
            // over the copy being pointed to.
            while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
                coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360;
            }

            new mapboxgl.Popup()
                .setLngLat(coordinates)
                .setHTML(description)
                .addTo(map);
        });

        // Change the cursor to a pointer when the mouse is over the places layer.
        map.on('mouseenter', 'current_point', function () {
            map.getCanvas().style.cursor = 'pointer';
        });

        // Change it back to a pointer when it leaves.
        map.on('mouseleave', 'current_point', function () {
            map.getCanvas().style.cursor = '';
        });

        for (var i = 0; i < 30; i++) {
            map.on('click', 'point' + i, function (e) {
                console.log('point' + i);
                var coordinates = e.features[0].geometry.coordinates.slice();
                var description = e.features[0].properties.description;
                if (e.defaultPrevented) return;
                // Ensure that if the map is zoomed out such that multiple
                // copies of the feature are visible, the popup appears
                // over the copy being pointed to.
                while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
                    coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360;
                }

                new mapboxgl.Popup()
                    .setLngLat(coordinates)
                    .setHTML(description)
                    .addTo(map);
            });

            // Change the cursor to a pointer when the mouse is over the places layer.
            map.on('mouseenter', 'point' + i, function () {
                map.getCanvas().style.cursor = 'pointer';
            });

            // Change it back to a pointer when it leaves.
            map.on('mouseleave', 'point' + i, function () {
                map.getCanvas().style.cursor = '';
            });
        }

    }

    // var staticDot = {
    //     width: small_size,
    //     height: small_size,
    //     data: new Uint8Array(small_size * small_size * 4),

    //     onAdd: function () {
    //         var canvas = document.createElement('canvas');
    //         canvas.width = this.width;
    //         canvas.height = this.height;
    //         this.context = canvas.getContext('2d');
    //     },

    //     render: function () {
    //         var duration = 1000;
    //         var t = (performance.now() % duration) / duration;

    //         var radius = small_size / 2 * 0.3;
    //         var context = this.context;

    //         // draw inner circle
    //         context.beginPath();
    //         context.arc(this.width / 2, this.height / 2, radius, 0, Math.PI * 2);
    //         context.fillStyle = 'rgba(255, 100, 100, 1)';
    //         context.strokeStyle = 'white';
    //         context.lineWidth = 2 + 4 * (1 - t);
    //         context.fill();
    //         context.stroke();

    //         // draw inner circle
    //         context.beginPath();
    //         context.arc(this.width / 2, this.height / 2, radius, 0, Math.PI * 2);
    //         context.fillStyle = 'rgba(0, 0, 255, 1)';
    //         context.fill();
    //         context.stroke();
    //         // update this image's data with data from the canvas
    //         this.data = context.getImageData(0, 0, this.width, this.height).data;

    //         // keep the map repainting
    //         map.triggerRepaint();

    //         // return `true` to let the map know that the image was updated
    //         return true;
    //     }
    // }

    var pulsingDot = {
        width: size,
        height: size,
        data: new Uint8Array(size * size * 4),

        onAdd: function () {
            var canvas = document.createElement('canvas');
            canvas.width = this.width;
            canvas.height = this.height;
            this.context = canvas.getContext('2d');
        },

        render: function () {
            var duration = 1000;
            var t = (performance.now() % duration) / duration;

            // var radius = size / 2 * 0.2;
            // var outerRadius = size / 2 * 0.3 * t + radius;
            // var context = this.context;

            var radius = (size / 2) * 0.3;
            var outerRadius = (size / 2) * 0.7 * t + radius;
            var context = this.context;
            // draw outer circle
            context.clearRect(0, 0, this.width, this.height);
            context.beginPath();
            context.arc(this.width / 2, this.height / 2, outerRadius, 0, Math.PI * 2);
            context.fillStyle = 'rgba(255, 200, 200,' + (1 - t) + ')';
            context.fill();

            // draw inner circle
            context.beginPath();
            context.arc(this.width / 2, this.height / 2, radius, 0, Math.PI * 2);
            context.fillStyle = 'rgba(255, 100, 100, 1)';
            context.strokeStyle = 'white';
            context.lineWidth = 2 + 4 * (1 - t);
            context.fill();
            context.stroke();

            // update this image's data with data from the canvas
            this.data = context.getImageData(0, 0, this.width / 2, this.height / 2).data;

            // keep the map repainting
            map.triggerRepaint();

            // return `true` to let the map know that the image was updated
            return true;
        }
    };

    _.OnRequestVehicleTracking = function (ID) {
        var pack = _.proxy.OnRequestVehicleTracking(ID);
        return pack;
    }

    _.OnClearVehicleTracking = function (deviceID) {
        var success = _.proxy.OnClearVehicleTracking(deviceID);
        return success;
    }

    _.ticksToDateString = function (ticks) {
        var epochTicks = 621355968000000000;
        var ticksPerMillisecond = 10000;
        var maxDateMilliseconds = 8640000000000000;
        if (isNaN(ticks)) {
            //      0001-01-01T00:00:00.000Z
            return "NANA-NA-NATNA:NA:BA.TMAN";
        }

        // convert the ticks into something javascript understands
        var ticksSinceEpoch = ticks - epochTicks;
        var millisecondsSinceEpoch = ticksSinceEpoch / ticksPerMillisecond;

        // output the result in something the human understands
        var date = new Date(millisecondsSinceEpoch);
        return date.toISOString();
    };
    //AddDot function is to add white/red dot on the map
    //id is the number of the dot
    //lng lat alt is the parameter of geolocation
    //type 1 is white dot type 2 is red dot
    AddDot = function (id, lng, lat, alt, type) {
        console.log("add new dot");
        map.on('load', function () {
            if (type == 1) {
                map.addSource('dot' + id, {
                    type: 'geojson',
                    data: {
                        "type": "Feature",
                        "geometry": {
                            "type": "Point",
                            "coordinates": [lng, lat]
                        }
                    }
                });

                map.addLayer({
                    "id": 'dot' + id,
                    "type": "circle",
                    "source": 'dot' + id,
                    'paint': {
                        'circle-radius': 8,
                        'circle-color': 'White'
                    }
                });

                console.log("dot111");
            }

            if (type == 2) {
                map.addSource('dot' + id, {
                    type: 'geojson',
                    data: {
                        "type": "Feature",
                        "geometry": {
                            "type": "Point",
                            "coordinates": [lng, lat]
                        }
                    }
                });
                map.addLayer({
                    "id": 'dot' + id,
                    "type": "circle",
                    "source": 'dot' + id,
                    "paint": {
                        'circle-radius': 8,
                        'circle-color': 'Red'
                    }
                });
            }
        });
    }
    //AddDirection function is to add direction path on the map
    //id is the id of the path
    //lng1 lat1 is the geolocation of starting dot
    //lng2 lat2 is the geolocation of ending dot
    //type1 is the straight path and type2 is drive path 
    AddDirection = function (id, lng1, lat1, lng2, lat2, type) {
        console.log("add new direction");
        map.on('load', function () {

            if (type == 1) {
                map.addSource('direction' + id, {
                    type: 'geojson',
                    data: {
                        "type": "Feature",
                        "properties": {},
                        "geometry": {
                            "type": "LineString",
                            "coordinates": [[lng1, lat1], [lng2, lat2]]
                        }
                    }
                });

                map.addLayer({
                    "id": 'direction' + id,
                    "type": "line",
                    "source": 'direction' + id,
                    "layout": {
                        "line-join": "round",
                        "line-cap": "round"
                    },
                    "paint": {
                        "line-color": "#68B1FA",
                        "line-width": 8
                    }
                });

            }

            if (type == 2) {
                _.DriveRoute([lng1, lat1], [lng2, lat2], id);
            }
        });
    }
    //clear dots and directions with id, 
    //the id does not distingush the type of markers on the map
    ClearMarker = function (id) {
        console.log("clear " + id);
        map.on('load', function () {
            if (map.getLayer('dot' + id)) {
                map.removeLayer('dot' + id);
            }
            if (map.getSource('dot' + id)) {
                map.removeSource('dot' + id);
            }
            if (map.getLayer('direction' + id)) {
                map.removeLayer('direction' + id);
            }
            if (map.getSource('direction' + id)) {
                map.removeSource('direction' + id);
            }
            if (map.getLayer("driveRoute" + id)) {
                map.removeLayer("driveRoute" + id);
            }
            if (map.getSource("driveRoute" + id)) {
                map.removeSource("driveRoute" + id);
            }
        });
    }
}



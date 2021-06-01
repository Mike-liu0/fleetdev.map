var Interface = function (name, methods) {
    if (arguments.length != 2) {
        throw new Error("Interface constructor called with " + arguments.length + "arguments, but expected exactly 2.");
    }
    this.name = name;
    this.methods = [];
    for (var i = 0, len = methods.length; i < len; i++) {
        if (typeof methods[i] !== 'string') {
            throw new Error("Interface constructor expects method names to be " + "passed in as a string.");
        }
        this.methods.push(methods[i]);
    }
};
// Static class method.
Interface.ensureImplements = function (object) {
    if (arguments.length < 2) {
        throw new Error("Function Interface.ensureImplements called with " + arguments.length + "arguments, but expected at least 2.");
    }
    for (var i = 1, len = arguments.length; i < len; i++) {
        var interface = arguments[i];
        if (interface.constructor !== Interface) {
            throw new Error("Function Interface.ensureImplements expects arguments" + "two and above to be instances of Interface.");
        }
        for (var j = 0, methodsLen = interface.methods.length; j < methodsLen; j++) {
            var method = interface.methods[j];
            if (!object[method] || typeof object[method] !== 'function') {
                throw new Error("Function Interface.ensureImplements: object " + "does not implement the " + interface.name + " interface. Method " + method + " was not found.");
            }
        }
    }
};

Map.prototype.Marker = function (id, lng, lat, alt, type) {


    map.addSource('point' + id, {
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
        "id": 'point' + id,
        "type": "circle",
        "source": 'point',
        'paint': {
            'circle-radius': 8,
            'circle-color': 'White'
        }
    });

}



var DynamicMap = new Interface('DynamicMap', ['centerOnPoint', 'zoom', 'draw']);

function displayRoute(mapInstance) {
    Interface.ensureImplements(mapInstance, DynamicMap);
    mapInstance.addMarker(1, 144.946457, -37.840935, 0, 1);
}

function Map() { }

var map = new Map();
// displayRoute(map);










Neddy.prototype.addMarker = function (lng, lat, alt, type) {
    this.lat = lat;
    this.lng = lng;
    this.alt = alt;
    this.type = type;
}

Neddy.prototype.addDirection = function (lng, lat, alt, type) {
    this.lat = lat;
    this.lng = lng;
    this.alt = alt;
    this.type = type;
}

// new instance and implement the method with argus
var neddy = new Neddy();
neddy.addMarker(144.946457, -37.840935, 0, 1);

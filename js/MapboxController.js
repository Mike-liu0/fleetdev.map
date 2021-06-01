
mapboxgl.accessToken = 'pk.eyJ1IjoieXVoZTA5MjUiLCJhIjoiY2tkbnJobWM2MHY2ZTJycWltNG1lbm5xNyJ9.OPHhWDcFai-mdAqNLzhATA';


var map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/streets-v11',
    center: [144.952723, -37.8145872], // starting position (melbourne CBD)
    zoom: 14, // starting zoom
});


// Add zoom and rotation controls to the map.
map.addControl(new mapboxgl.NavigationControl());

// Add geolocate control to the map.
map.addControl(
    new mapboxgl.GeolocateControl({
        positionOptions: {
            enableHighAccuracy: true
        },
        trackUserLocation: true
    })
);


// Hard Code Example
var html_Request = $('#request_1').html();
var html_Post = $('#post_1').html();



var popup_Request = new mapboxgl.Popup({ offset: 4 }).setHTML(html_Request);
var popup_Post = new mapboxgl.Popup({ offset: 4 }).setHTML(html_Post);

// Add default marker
var marker_Request = new mapboxgl.Marker()
    .setLngLat([144.952723, -37.8145872])
    .setPopup(popup_Request) // sets a popup on this marker
    .addTo(map);



var marker_Post = new mapboxgl.Marker()
    .setLngLat([144.970723, -37.8169872])
    .setPopup(popup_Post) // sets a popup on this marker
    .addTo(map);






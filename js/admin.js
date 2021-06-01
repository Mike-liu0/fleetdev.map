/* ============================================================================
 * File: admin.html
 * Author: Deyou Zou
 * Date: 7 Jan 2021
 * Project: Sycamore - ALLINK
 ==============================================================================
 */

document.getElementById("postviewer").addEventListener("click", inputPostGuid);

function inputPostGuid() {
    // Change the header mark to back button
    document.getElementById("homeMark").style.display = "none";
    document.getElementById("backMark").style.display = "block";
    document.getElementById("backMark").addEventListener("click", function () {
        document.getElementById("contents").style.display = "block";
        document.getElementById("postGuidInput").style.display = "none";
        document.getElementById("noGuidMessage").style.display = "none";
        document.getElementById("homeMark").style.display = "block";
        document.getElementById("backMark").style.display = "none";
    })


    document.getElementById("noGuidMessage").style.display = "none";
    document.getElementById("contents").style.display = "none";
    document.getElementById("postGuidInput").style.display = "block";
    document.getElementById("confirmbutton").addEventListener("click", function () {
        var guid = document.getElementById("inputbox").value;
        if (guid == "") {
            document.getElementById("noGuidMessage").style.display = "block";
        } else {
            var link = 'post.html?guid=' + guid;
            location.href = link;
        }
    })
    
}
function OAuthClient(url){
    this.url = url;
}

OAuthClient.prototype.createImplicitFlowRequest = function (clientid, callback, scope, responseType) {
    responseType = responseType || "token";

    var state = (Date.now() + Math.random()) * Math.random();
    state = state.toString().replace(".", "");
    var nonce = (Date.now() + Math.random()) * Math.random();
    nonce = nonce.toString().replace(".", "");

    var url =
        this.url + "?" + 
        "client_id=" + encodeURIComponent(clientid) + "&" + 
        "redirect_uri=" + encodeURIComponent(callback) + "&" + 
        "response_type=" + encodeURIComponent(responseType) + "&" +
        "scope=" + encodeURIComponent(scope) + "&" + 
        "state=" + encodeURIComponent(state) + "&" + 
        "nonce=" + encodeURIComponent(nonce);

    return {
        url:url, 
        state: state,
        nonce: nonce
    };
};

OAuthClient.prototype.parseResult = function (queryString) {
    var params = {},
        //queryString = location.hash.substring(1),
        regex = /([^&=]+)=([^&]*)/g,
        m;

    while (m = regex.exec(queryString)) {
        params[decodeURIComponent(m[1])] = decodeURIComponent(m[2]);
    }

    for (var prop in params) {
        return params;
    }
};

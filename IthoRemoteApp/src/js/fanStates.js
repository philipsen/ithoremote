fromStream('newstream')
.when({
    $init: function(){
        return {
            state: {}
        };
    },
    IthoControlBoxEvent: function(s,e) {
        //log("triggered fanspeed");
        // log(JSON.stringify(e));
        var ids = e.data.id;
        var name = ids[0].toString(16) + ":" + ids[1].toString(16);
        //log("name = " + name);
        if (typeof s.state[name] === 'undefined') {
            s.state[name] = {};
            s.state[name]["count"] = 0;
        }
        s.state[name]["count"] = s.state[name]["count"] + 1;
        s.state[name]["rssi"] = e.data.rssi;
        s.state[name]["fanspeed"] = e.data.fanspeed;
        s.state[name]["sender"] = e.data.sender;
        s.state[name]["time"] = new Date();
        //log("name = " + name + "\n" + JSON.stringify(s.state[name]));

        if (e.data.house !== null) {
            s.state[name]["name"] = e.data.house.Fields[0];
        }
    },
}).outputState();

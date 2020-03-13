fromStream('newstream')
.when({
    $init: function(){
        return {
            count: 0,
            state: {}
        };
    },
    IthoControlBoxEvent: function(s,e) {
        //log("triggered fanspeed");
        //log(JSON.stringify(e));
        var ids = e.data.id;
        var name = ids[0].toString(16) + ":" + ids[1].toString(16);
        //log("name = " + name);
        var fs = { 
            "rssi": e.data.rssi, 
            "speed": e.data.fanspeed
        };
        if (e.data.house !== null) {
            fs.name = e.data.house.Fields[0];
        }
        s.state[name] = fs;
        s.count = s.count + 1;
    },
}).outputState();


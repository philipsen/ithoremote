const states = {
    unknown: 'unknown',
    eco: 'eco',
    comfort: 'comfort',
    kitchen: 'kitchen',
    timer: 'timer',
    timerWc: 'timerWc',
    wc: 'wc beneden'
    };

fromStream('newstream')
.partitionBy(function(e) { return e.data.house; } )
.when({
    $init: function(){
        return {
            transmits: 0,
            fanspeed: 0,
            baseState: states.unknown,
            state: states.unknown
        };
    },
    IthoFanSpeed: function(s,e) {
        // log("triggered fanspeed");
        //log(JSON.stringify(e));
        s.fanspeed = e.data.speed;
    },
    IthoTransmitEvent: function(s,e){
        // log("triggered");
        // log(JSON.stringify(e));
        s.transmits += 1
        switch (e.data.remote) {
            case 'main':
                switch (e.data.command) {
                    case 'eco':
                        s.state = states.eco;
                        s.baseState = states.eco;
                        break;
                    case 'comfort':
                        s.state = states.comfort;
                        s.baseState = states.comfort;
                        break;
                    case 'cook1':
                    case 'cook2':
                        s.state = states.kitchen;
                        break;
                    case 'timer1':
                    case 'timer2':
                    case 'timer3':
                        //("old base = " + s.state + ":" + s.baseState + ":")
                        //s.baseState = s.state;
                        s.state = states.timer
                }
                //s.state = "main";
                break;
            case 'second':
                switch (e.data.command) {
                    case 's_timer1':
                    case 's_timer2':
                    case 's_timer3':
                        s.state = states.timerWc;
                        break;
                }
        }
    },
    IthoTransmitCancelEvent: function(s,e){
        log("cancel " + e.data.house + " " + e.data.cancelCommand);
        s.state = s.baseState;
        //s.baseState = states.eco;
    }
}).outputState().outputTo("status");

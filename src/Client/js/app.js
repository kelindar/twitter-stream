var emitter = emitter.connect({
    secure: true
}); 
var key = 'jtdQO-hb5jfujowvIKvSF41NeQOE8IoF';
var vue = new Vue({
    el: '#app',
    data: {
        messages: []
    }
});

emitter.on('connect', function(){
    // once we're connected, subscribe to the 'tweet-stream' channel
    console.log('emitter: connected');
    emitter.subscribe({
        key: key,
        channel: "tweet-stream"
    });
})

// on every message, print it out
emitter.on('message', function(msg){

    // log that we've received a message
    console.log('emitter: received ' + msg.asString() );

    // If we have already 5 messages, remove the oldest one (first)
    if (vue.$data.messages.length >= 7){
        vue.$data.messages.shift();
    }

    // Push the message we've received and update an identicon once it's there
    vue.$data.messages.push(msg.asObject());
});
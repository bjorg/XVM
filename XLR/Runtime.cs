using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

using Id = System.String;
using Sel = System.Int32;

namespace XL.Runtime {

    public delegate void ReceiverHandler(object context, object message);
    public delegate void ProcessHandler(object context);

    public sealed class Global {

        //--- Class Fields ---
        [ThreadStatic]
        public static Scheduler Scheduler;

        //--- Constructors ---
        private Global() {}
    }

    public class Scheduler {

        //--- Types ---
        private class Entry {

            //--- Fields ---
            public object Context;
            public ProcessHandler Handler;

            //--- Constructors ---
            public Entry(object context, ProcessHandler handler) {
                this.Context = context;
                this.Handler = handler;
            }
        }

        //--- Fields ---
        private Queue queue = Queue.Synchronized(new Queue());

        //--- Methods ---
        public void Fork(object context, ProcessHandler handler) {
            queue.Enqueue(new Entry(context, handler));
        }

        public void Run() {
            Global.Scheduler = this;
            while(queue.Count > 0) {
                Entry current = (Entry)queue.Dequeue();
                current.Handler(current.Context);
            }
            Global.Scheduler = null;
        }
    }

    public abstract class Port {

        //--- Properties ---
        public abstract Channel this[Sel selector] { get; }

        //--- Methods ---
        public void Receive(Sel selector, object context, ReceiverHandler handler) {
            this[selector].Receive(this, context, handler);
        }

        public void Send(Sel selector, object value) {
            this[selector].Send(this, value);
        }
    }

    public class GenericPort : Port {

        //--- Fields ---
        private Channel[] channels;

        //--- Constructors ---
        public GenericPort(Channel[] channels) {
            this.channels = channels;
        }

        //--- Properties ---
        public override Channel this[Sel selector] {
            get {
                return channels[selector];
            }
        }
    }

    public sealed class ClrInstancePort : GenericPort {

        //--- Fields ---
        private object target;

        //--- Constructors ---
        public ClrInstancePort(Runtime.Channel[] channels, object target) : base(channels) {
            this.target = target;
        }

        //--- Properties ---
        public object Target {
            get {
                return target;
            }
        }
    }

    public sealed class ClrClassPort : GenericPort {

        //--- Constructors ---
        public ClrClassPort(Runtime.Channel[] channels) : base(channels) {}
    }

    public abstract class Channel {

        //--- Methods ---
        public abstract void Receive(Port port, object context, ReceiverHandler handler);
        public abstract void Send(Port port, object value);
    }

    public class GenericChannel : Channel {

        //--- Types ---
        private enum Polarity {
            EMPTY_FIELD,
            EMPTY_QUEUE,
            SINGLE_SENDER,
            MULTIPLE_SENDERS,
            SINGLE_RECEIVER,
            MULTIPLE_RECEIVERS
        }

        private class Entry {

            //--- Fields ---
            public object Context;
            public ReceiverHandler Handler;

            //--- Constructors ---
            public Entry(object context, ReceiverHandler handler) {
                this.Context = context;
                this.Handler = handler;
            }
        }

        //--- Fields ---
        private object contents;
        private Polarity polarity = Polarity.EMPTY_FIELD;

        //--- Properties ---
        protected object SyncRoot {
            get {
                return this;
            }
        }

        //--- Methods ---
        public override void Receive(Port port, object context, ReceiverHandler handler) {
            object found = null;
            Queue queue;
            lock(SyncRoot) {
                switch(polarity) {
                case Polarity.EMPTY_FIELD:
                    polarity = Polarity.SINGLE_RECEIVER;
                    contents = new Entry(context, handler);
                    break;
                case Polarity.EMPTY_QUEUE:
                    polarity = Polarity.MULTIPLE_RECEIVERS;
                    ((Queue)contents).Enqueue(new Entry(context, handler));
                    break;
                case Polarity.SINGLE_SENDER:
                    polarity = Polarity.EMPTY_FIELD;
                    found = contents;
                    contents = null;
                    break;
                case Polarity.MULTIPLE_SENDERS:
                    queue = (Queue)contents;
                    found = queue.Dequeue();
                    if(queue.Count == 0) {
                        polarity = Polarity.EMPTY_QUEUE;
                    }
                    break;
                case Polarity.SINGLE_RECEIVER:
                    polarity = Polarity.MULTIPLE_RECEIVERS;
                    queue = new Queue(8);
                    queue.Enqueue(contents);
                    queue.Enqueue(new Entry(context, handler));
                    contents = queue;
                    break;
                case Polarity.MULTIPLE_RECEIVERS:
                   ((Queue)contents).Enqueue(new Entry(context, handler));
                    break;
                default:
                    Debug.Fail("unexpected");
                    break;
                }
            }
            if(found != null) {
                handler(context, found);
            }
        }

        public override void Send(Port port, object value) {
            Entry found = null;
            Queue queue;
            lock(SyncRoot) {
                switch(polarity) {
                case Polarity.EMPTY_FIELD:
                    polarity = Polarity.SINGLE_SENDER;
                    contents = value;
                    break;
                case Polarity.EMPTY_QUEUE:
                    polarity = Polarity.MULTIPLE_SENDERS;
                    ((Queue)contents).Enqueue(value);
                    break;
                case Polarity.SINGLE_SENDER:
                    polarity = Polarity.MULTIPLE_SENDERS;
                    queue = new Queue(8);
                    queue.Enqueue(contents);
                    queue.Enqueue(value);
                    contents = queue;
                    break;
                case Polarity.MULTIPLE_SENDERS:
                    ((Queue)contents).Enqueue(value);
                    break;
                case Polarity.SINGLE_RECEIVER:
                    polarity = Polarity.EMPTY_FIELD;
                    found = (Entry)contents;
                    contents = null;
                    break;
                case Polarity.MULTIPLE_RECEIVERS:
                    queue = (Queue)contents;
                    found = (Entry)queue.Dequeue();
                    if(queue.Count == 0) {
                        polarity = Polarity.EMPTY_QUEUE;
                    }
                    break;
                default:
                    Debug.Fail("unexpected");
                    break;
                }
            }
            if(found != null) {
                found.Handler(found.Context, value);
            }
        }
    }

    public abstract class ClrMethodChannel : Channel {

        //--- Methods ---
        public override void Receive(Port port, object context, ReceiverHandler handler) {
            throw new NotSupportedException("receive on clr channel");
        }
    }
}


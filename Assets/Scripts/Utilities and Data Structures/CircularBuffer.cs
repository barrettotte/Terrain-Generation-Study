using System;
using System.Collections.Generic;
using System.Collections;


public class CircularBuffer<T> {
	Queue<T> queue;
	int size;

	public CircularBuffer (int s) {
		queue = new Queue<T> (s);
		this.size = s;
	}

	public void Add (T obj) {
		if (queue.Count == size) {
			queue.Dequeue ();
			queue.Enqueue (obj);
		}
		else{
			queue.Enqueue (obj);
		}
	}

	public T Read () {
		return queue.Dequeue ();
	}

	public T Peek () {
		return queue.Peek ();
	}
}
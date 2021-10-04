# concurrentfileprocessor

Design of concurrent file processor:
---------------------------------------
1) We have the groups to be fixed so we can use 3 concurrent queues for each group where the filenames to be processed in order are stored
   based on what time they arrived.
2) We need 2 asynchronous services/modules/routine
   - 1 for reading the folder continuously and notifying when a new file is added
     For this purpose we can use in built FileSystemWatcher and register a callback for create which can then act on the file created.
     So everytime a file is created, the callback performs a parse operation on the filename to identify which group the arriving
     file belongs to and enqueues the filename (metadata)  to that specific queue corresponding to that group.
   - 1 for dequeueing the filenames from each queue in a roundrobin order. So as and when the filenames are dequeued from the respective
     queues, we can append them to a "Task list" which is an array of the number of parallel processings allowed and those many file
     transformation processes will be performed in parallel.
3) Since we want the file transformation algo to be modifiable in future, we can have an enum of sorts which can represent the type of algo
   and based on that choose the corresponding implementation.

Considerations/assumptions:
1) Dequeueing from 3 queues.
   - go through the queues in round robin , fetch the head element filename and and push the trasform algo task 
     into the tasks array (consists fo 3 elements max, which is configurable)
   - In case there arent enough elelments or files to be fill up the array, then wait for a timer and then
     just break out of the loop to go ahead and execute whatever tasks are available.

2) Totally wait for 30 seconds for the dequeue logic to run, which can be configured or changed later.

3) The input path for files to be created and the output path for processeed files to be created are assumed to be fixed for now. 
    We can changethe private variables or the program can be extended to take in input params for folder locations.


Extensions: A separate program can be written to test the application instead of manually copying the files and testing

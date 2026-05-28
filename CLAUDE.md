## Error Handling
- If you encounter a JSON parse error like "Expecting value: line 1 column 1", 
  this means an API response was empty. Retry the same operation once automatically.
- Never pause and wait for user input on transient API errors - just retry and continue.
- If a grep returns an error, skip it and continue with the next planned step.
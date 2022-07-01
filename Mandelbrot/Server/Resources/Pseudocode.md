## Pseudo code for Mandelbrot algorithm

#### Escape time algorithm 
Checks how many iterations it takes for a point to 'explode' (if it ever does), for coloring the pixels
```
calculate escape iterations (original point 'pt'){
	scale the original point to lie within mandelbrot boundaries (or use fancy numero system??)

	make temp point to iterate on (pt1) (0,0)
	
	store current iterations (0)
	set iteration limit (200?)

	while (pt1 squared is inside the circle AND we havent reached iteration limit) {
        xtemp = pt1.x squared - pt1.y squared + pt.x
		pt1.y = 2 * pt1 + pt.y
		pt1.x = xtemp
		iteration++
	}
	return number of iterations
}

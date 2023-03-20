# GitHubExt - by Michael Haber

## About this project
Sample project that extends existing GitHub user search functionality including some basic user detail.

## Future Considerations (TODO)
* If we have to revisit this, include robust unit testing. For ongoing maintenance, respecting unit testing becomes increasingly necessary to avoid regression defects
* Let's consult with a ux designer on an optimal experience
	* Enhance design for better mobile experience i.e. full width page elements
* Establish different environments depending on client/team needs (dev, test, prod, etc)
* Establish parameter store or similar to hold various constants, keys and other 
	* Environment-specific values
* Depending on client's request, include ability to customize search:
	* Adjust number of paginated results
	* Result ordering
	* Search filter
* Handle GitHub API search limitation more gracefully 
	* Cache last succesful result, so something is returned
	* Disable hitting GitHub API when limit about to be exceeded (avoid abuse) proactively avoid exceeding API limit
	* Provide insight to end user on when search is available again (display timer)
	* Perhaps establish account relations with GitHub to improve or eliminate limiting

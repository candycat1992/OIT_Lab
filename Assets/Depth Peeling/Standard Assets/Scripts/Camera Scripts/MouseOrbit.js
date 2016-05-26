var target : Transform;
var distance = 10.0;

var xSpeed = 250.0;
var ySpeed = 120.0;

var yMinLimit = -20;
var yMaxLimit = 80;

private var x = 0.0;
private var y = 0.0;

@script AddComponentMenu("Camera-Control/Mouse Orbit")

function Start () {
    var angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;

	// Make the rigid body not change rotation
   	if (GetComponent.<Rigidbody>())
		GetComponent.<Rigidbody>().freezeRotation = true;
}

function LateUpdate () {
	if (target && (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))) {
        x += Input.GetAxis("Mouse X") * xSpeed * 0.02;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02;
    }

    if (target) {
 		y = ClampAngle(y, yMinLimit, yMaxLimit);
 		       
        var rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
        
        transform.rotation = rotation;
        transform.position = position;
    }
    
    if(Input.GetKey(KeyCode.A))
    {
    	distance = Mathf.Max(2, distance - Time.deltaTime * 2.0f);
    }
    if(Input.GetKey(KeyCode.Z))
    {
    	distance = Mathf.Min(50, distance + Time.deltaTime * 2.0f);
    }
}

static function ClampAngle (angle : float, min : float, max : float) {
	if (angle < -360)
		angle += 360;
	if (angle > 360)
		angle -= 360;
	return Mathf.Clamp (angle, min, max);
}
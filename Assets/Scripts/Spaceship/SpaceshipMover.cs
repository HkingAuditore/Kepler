using UnityEngine;

namespace Spaceship
{
    public class SpaceshipMover : MonoBehaviour
    {
        public Transform avatar;                //船体
        public float     maxForwardSpeed = 10f; //最大前进速度
        public float     forwoardAcc     = 5f;  //前进加速度
        public float     forwardDamp     = 5f;  //前进阻尼

        public float maxUpSpeed = 10f; //最大上升速度
        public float upAcc      = 5f;  //最大上升加速度
        public float upDamp     = 5f;  //上升阻尼

        public float maxRotateSpeed = 10f; //最大旋转速度
        public float rotateAcc      = 5f;  //旋转加速度
        public float rotateDamp     = 5f;  //旋转阻尼

        public float avatarUpAngle            = 5f; //船体上升倾角
        public float avatarRotateAngle        = 5f; //船体旋转倾角
        public float avatarAngleInterpolation = 5f; //倾角转动差值

        internal Vector3 moveDirection;   //移动向量
        internal float   nowForwardSpeed; //当前前向移动速度
        internal float   nowUpSpeed;      //当前上升速度
        internal float   nowRotateSpeed;  //当前旋转速度
        internal float   nowRotateAngle;  //当前飞船转角

        public Vector4 inputVector;

        private Rigidbody m_rigidbody;

        // Start is called before the first frame update
        private void Start()
        {
            m_rigidbody    = GetComponent<Rigidbody>();
            nowRotateAngle = transform.rotation.eulerAngles.y;
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void FixedUpdate()
        {
            // Debug.Log("Rotation" + nowRotateAngle);
            // Debug.DrawLine(this.transform.position,
            //                this.transform.position + m_rigidbody.velocity,
            //                Color.cyan);
            WaitForInputFinish();
            if (avatar != null)
            {
                //船体倾角模拟
                avatar.localRotation = Quaternion.Lerp(avatar.localRotation,
                                                       Quaternion.Euler(avatarUpAngle * -(nowUpSpeed / maxUpSpeed),
                                                                        0,
                                                                        avatarRotateAngle *
                                                                        -(nowRotateSpeed / maxRotateSpeed)),
                                                       avatarAngleInterpolation * Time.deltaTime);

                if(_timer > 4f && this.inputVector.magnitude <= .0005)
                    FaceToVelocity();
            }

            OnMove();
        }

        private void FaceToVelocity()
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.LookRotation(m_rigidbody.velocity),
                                                       .5f * Time.fixedDeltaTime);
            nowRotateAngle = this.transform.localEulerAngles.y;

        }

        private void OnMove()
        {
            //旋转计算
            if (inputVector.y > float.Epsilon || inputVector.y < -float.Epsilon)
                nowRotateSpeed += rotateAcc * Time.fixedDeltaTime * inputVector.y;
            else
                nowRotateSpeed = Mathf.Lerp(nowRotateSpeed, 0, rotateDamp * Time.fixedDeltaTime);
            nowRotateSpeed = Mathf.Clamp(nowRotateSpeed, -maxRotateSpeed, maxRotateSpeed);
            nowRotateAngle = nowRotateAngle + nowRotateSpeed;
            m_rigidbody.MoveRotation(Quaternion.Lerp(
                                                     transform.rotation,
                                                     Quaternion.Euler(0, nowRotateAngle, 0),
                                                     rotateDamp * Time.fixedDeltaTime)
                                    );

            //移动计算
            if (inputVector.x > float.Epsilon || inputVector.x < -float.Epsilon)
                //nowForwardSpeed += forwoardAcc * Time.fixedDeltaTime * input.z;
                nowForwardSpeed = Mathf.Lerp(nowForwardSpeed, nowForwardSpeed + forwoardAcc * inputVector.x,
                                             Time.fixedDeltaTime);
            else
                nowForwardSpeed = Mathf.Lerp(nowForwardSpeed, 0, forwardDamp * Time.fixedDeltaTime);
            nowForwardSpeed = Mathf.Clamp(nowForwardSpeed, -maxForwardSpeed, maxForwardSpeed);
            m_rigidbody.AddForce(transform.forward * nowForwardSpeed);


            if (inputVector.z > float.Epsilon || inputVector.z < -float.Epsilon)
                nowUpSpeed = Mathf.Lerp(nowUpSpeed, nowUpSpeed + upAcc * inputVector.z, Time.fixedDeltaTime);
            else
                nowUpSpeed = Mathf.Lerp(nowUpSpeed, 0, upDamp * Time.fixedDeltaTime);
            nowUpSpeed = Mathf.Clamp(nowUpSpeed, -maxUpSpeed, maxUpSpeed);

            // moveDirection = transform.up * nowUpSpeed + transform.forward * nowForwardSpeed;
            // m_rigidbody.MovePosition(transform.position + moveDirection);
        }

        /// <summary>
        ///     围绕某点旋转指定角度
        /// </summary>
        /// <param name="position">自身坐标</param>
        /// <param name="center">旋转中心</param>
        /// <param name="axis">围绕旋转轴</param>
        /// <param name="angle">旋转角度</param>
        /// <returns></returns>
        private Vector3 RotateRound(Vector3 position, Vector3 center, Vector3 axis, float angle)
        {
            return Quaternion.AngleAxis(angle, axis) * (position - center) + center;
        }

        private float _timer = 0f;
        public void WaitForInputFinish()
        {
            if(this.inputVector.magnitude <= .0005)
            {
                _timer += Time.fixedDeltaTime;
            }
            else
            {
                _timer = 0f;
            }            
        }
    }
}
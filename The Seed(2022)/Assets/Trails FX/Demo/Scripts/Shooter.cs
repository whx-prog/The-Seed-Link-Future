using UnityEngine;

namespace TrailsFX.Demos {

    public class Shooter : MonoBehaviour {

        public float timeInterval = 0.3f;
        public GameObject[] bulletPrefabs;

        Quaternion targetRot;
        float lastTargetTime;
        Vector3 lookAt, previousLookAt;
        GameObject[] bulletPool;
        int poolIndex;
        Vector3 startPos;

        void Start() {
            startPos = transform.position;
            bulletPool = new GameObject[20];
            previousLookAt = Vector3.up;
            NewTarget();
        }

        void Update() {
            float deltaTime = Time.deltaTime;
            if (Vector3.Distance(startPos, transform.position) < 0.002f) {
                if (Time.time - lastTargetTime > timeInterval) {
                    NewTarget();
                    Shoot();
                }
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, deltaTime * 5f);
            } else {
                lastTargetTime = Time.time;
            }
            transform.position = Vector3.Lerp(transform.position, startPos, deltaTime * 4f);
        }

        void NewTarget() {
            lookAt = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), Random.Range(-1f, 1f));
            targetRot.SetFromToRotation(previousLookAt, lookAt);
            previousLookAt = lookAt;
            lastTargetTime = Time.time;
        }

        void Shoot() {
            if (++poolIndex >= bulletPool.Length) {
                poolIndex = 0;
            }
            GameObject bullet = bulletPool[poolIndex];
            if (bulletPool[poolIndex] == null) {
                GameObject bulletPrefab = bulletPrefabs[Random.Range(0, bulletPrefabs.Length)];
                bullet = Instantiate<GameObject>(bulletPrefab);
                bulletPool[poolIndex] = bullet;
            }
            Vector3 cannonTip = transform.TransformPoint(new Vector3(0, 1.1f, 0));
            Vector3 direction = (cannonTip - transform.position).normalized;
            transform.position -= direction * 0.05f;
            bullet.transform.position = cannonTip;
            bullet.GetComponent<Rigidbody>().velocity = direction * (2f + Random.value);
            bullet.GetComponent<Renderer>().enabled = true;

            TrailEffect trail = bullet.GetComponent<TrailEffect>();
            trail.Clear();
            trail.duration = 0.5f + (Random.value * 2f);
        }
    }

}
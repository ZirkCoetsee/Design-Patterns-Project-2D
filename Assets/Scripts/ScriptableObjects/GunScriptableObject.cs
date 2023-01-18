using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    //public ImpactType ImpactType;
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;
    public ShootConfigurationScriptableObject ShootConfig;
    public TrailConfigurationScriptableObject TrailConfig;
    public DamageConfigScriptableObject DamageConfig;

    private MonoBehaviour ActiveMonobehaviour;
    private GameObject Model;
    private float LastShootTime;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour)
    {
        this.ActiveMonobehaviour = ActiveMonoBehaviour;
        LastShootTime = 0; // in editor this will no be properly reset, in build it's fine
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
    }

    public void Shoot()
    {
        if (Time.time > ShootConfig.fireRate + LastShootTime)
        {
            LastShootTime = Time.time;
            ShootSystem.Play();
            //Vector3 shootDirection = ShootSystem.transform.forward 
            //    + new Vector3(
            //        Random.Range(-ShootConfig.Spread.x,ShootConfig.Spread.x),
            //        Random.Range(-ShootConfig.Spread.y, ShootConfig.Spread.y),
            //        Random.Range(-ShootConfig.Spread.z, ShootConfig.Spread.z));

            //shootDirection.Normalize();

            Vector2 shootDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - ShootSystem.transform.position;
            Debug.DrawRay(ShootSystem.transform.position, shootDirection, Color.red, float.MaxValue);

            RaycastHit2D hit = Physics2D.Raycast(ShootSystem.transform.position, shootDirection, float.MaxValue, ShootConfig.HitMask, float.MinValue);
            //If the ray cast does hit a object
            if (hit)
            {
                //Play the shooting trail animation
                ActiveMonobehaviour.StartCoroutine(
                    PlayTrail(ShootSystem.transform.position, hit.point, hit)
                );
            }else
            {
                //Play the shooting trail animation with the maximum distance set to the miss distance
                ActiveMonobehaviour.StartCoroutine(
                    PlayTrail(new Vector2(ShootSystem.transform.position.x, ShootSystem.transform.position.y),
                    new Vector2(ShootSystem.transform.position.x, ShootSystem.transform.position.y) + (shootDirection * TrailConfig.MissDistance), new RaycastHit2D()));
            }
        }
    }

    private IEnumerator PlayTrail(Vector2 StartPoint, Vector2 EndPoint, RaycastHit2D raycastHit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null; // wait a frame to avoid position carry over from the last frame if reused

        instance.emitting = true;

        float distance = Vector2.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while(remainingDistance > 0)
        {
            instance.transform.position = Vector2.Lerp(StartPoint, EndPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = EndPoint;

    //TODO: can add a surface impact system
        if (raycastHit.collider != null)
        {
            //SurfaceManager.Instance.HandleImpact(
            //    Hit.transform.gameObject,
            //    EndPoint,
            //    Hit.normal,
            //    ImpactType,
            //    0)
        Debug.Log("Raycast hit: " + raycastHit.collider.gameObject);

            if (raycastHit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(DamageConfig.GetDamage(distance));
            }
        }

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);

    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Projectile Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;

    }
}

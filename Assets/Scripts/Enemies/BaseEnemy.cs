using System;
using Cysharp.Threading.Tasks;
using Interfaces;
using RoomBehaviour.Traps;
using Signals;
using UnityEngine;
using Utils.Signals;
using Random = UnityEngine.Random;

namespace Enemies
{
    
    public abstract class BaseEnemy : MonoBehaviour, IDamageable, IPitTrapInteracting, ISlowTrapInteracting
    {
        [SerializeField] protected EnemyStats enemyStats;
        [SerializeField] protected AudioClip[]enemyAudio;
        protected abstract bool isFlyingEnemy { get; }
        protected Vector3 moveDirection;
        protected Rigidbody2D rb;
        protected AudioSource audio;
        protected Animator anim;
        public Transform Player { get; set; }
        public bool Active { get; set; }
        private float _trapMovK = 1f;
        protected Vector3 DirectionToPlayer
        {
            get
            {
                if(Player)
                {
                    return (Player.position - transform.position).normalized;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }
        public event Action<Vector3> onDie;

        private void FixedUpdate()
        {
            Debug.DrawRay(transform.position, moveDirection, Color.black);
            if (Active)
            {
                rb.velocity = moveDirection * enemyStats.Speed * _trapMovK;
            }
            else
            {
                rb.velocity = RecliningDirection;
            }
        }

        protected virtual void Awake()
        {
            audio = GetComponent<AudioSource>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
        }

        public async void Damage(float damage)
        {
            
            if (enemyAudio.Length > 0)
            {
                var num = Random.Range(0, enemyAudio.Length);
                audio.clip = enemyAudio[num];
                audio.Play();
            }
            anim.SetTrigger("Damage");
            Active = false;
            enemyStats.Health -= damage;
            Debug.Log($"{name} got {damage} for player, it's current health = {enemyStats.Health}");
            if (enemyStats.Health <= 0)
            {
                Death();
                await Die();
                onDie?.Invoke(transform.position);
                return;
            }
            await UniTask.Delay((int)(1000 * enemyStats.StunTime));
            
            if (!this)
                return;
            Active = true;
        }
        
        protected virtual void Death()
        {
            GetComponent<Collider2D>().enabled = false;
        }
        
        public void Damage(float damage,Vector2 directionReclining)
        {
            Damage(damage);
            RecliningEnemy(directionReclining, damage);
        }
        private Vector2 RecliningDirection;
        
        public async void RecliningEnemy(Vector2 direction, float strength)
        {
            
          
            RecliningDirection = direction.normalized * strength * enemyStats.RecliningValue;
            await UniTask.Delay((int) (1000 * 0.5));
            RecliningDirection = Vector2.zero;
        }
        
        protected virtual async UniTask Die()
        {
            SignalBus.Invoke(new EnemyDieSignal());
            await UniTask.Delay((int)(1000 * .5f));
            if (gameObject) Destroy(gameObject);
        }

        protected virtual void TryDamagePlayer(Collider2D col)
        {
            if(!col.CompareTag("Player") || !Active)
                return;

            var damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.Damage(enemyStats.Damage,transform.position-col.transform.position);
            }
        }

        #region Traps
        public void PitTrap(PitTrap trap)
        {
            if (isFlyingEnemy) return;
            
            Destroy(gameObject);
        }

        public void SlowTrap(bool entered, SlowTrap trap)
        {
            if (isFlyingEnemy) return;
            if (entered)
            {
                _trapMovK = trap.SpeedMult;
            }
            else
            {
                _trapMovK = 1f;
            }
        }

        public void NeedlesTrap(Needles trap)
        {
            if (isFlyingEnemy) return;
            Damage(trap.damage);
        }

        #endregion
    }
}
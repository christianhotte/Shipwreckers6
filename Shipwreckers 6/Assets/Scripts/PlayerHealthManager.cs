using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealthManager : MonoBehaviour
{
    //Objects & Components:
    /// <summary>Singleton instance of this script in scene.</summary>
    public static PlayerHealthManager main;
    private Transform handModel;              //Transform of hand model
    private SkinnedMeshRenderer handRenderer; //Renderer component on player hand
    private AudioSource audioSource;          //Audio source component on hand

    //Settings:
    [Header("Damage Animation Settings:")]
    [SerializeField] [Tooltip("Sound which plays when player is hurt")]                                        private AudioClip hurtSound;
    [SerializeField] [Tooltip("Color the hand will flash when player is damaged")]                             private Color hurtFlashColor;
    [SerializeField] [Tooltip("Percentage hand scale is increased by with each flash when player is damaged")] private float hurtFlashScalePercent;
    [SerializeField] [Tooltip("How long to play flashing animation for when player is damaged")]               private float hurtFlashTime;
    [SerializeField] [Tooltip("Number of times to flash hand red during hurt animation")]                      private int hurtFlashes;

    //Runtime Vars:
    /// <summary>
    /// How much health the player currently has (player will die if this goes negative).
    /// </summary>
    public static int currentHealth = 5;
    private int animHealth = 5;        //How much health player currently has, according to animated visuals
    private Color[] handMatOrigColors; //Starting colors of materials in player hand
    private Vector3 handOrigScale;     //Starting scale of hand model
    private Vector3 handTargScale;     //Target scale when growing hand model for hurt animation

    //Coroutines:
    IEnumerator HurtFlash()
    {
        //Initialize:
        int hurtFlashesLeft = (hurtFlashes * 2) - 1;       //Get total number of flashes to show on hand throughout animation
        float flashTime = hurtFlashTime / hurtFlashesLeft; //Get amount of time to play each flash for

        //Animate flashes:
        for(; hurtFlashesLeft >= 0; hurtFlashesLeft -= 1) //Iterate through each designated flash sequence
        {
            for (float timeLeft = flashTime; timeLeft > 0; timeLeft -= Time.fixedDeltaTime) //Flash sequence for each individual flash
            {
                //Compute interpolant:
                float t = timeLeft / flashTime;          //Get interpolant value from time remaining
                if (hurtFlashesLeft % 2 != 0) t = 1 - t; //Flip interpolant value if flashing toward red (flash number is even)

                //Modify hand properties:
                for (int m = 0; m < handRenderer.materials.Length; m++) //Iterate through each material on player hand (for modifying color)
                {
                    handRenderer.materials[m].color = Color.Lerp(handMatOrigColors[m], hurtFlashColor, t); //Interpolate color between flash color and material's original color (depending on interpolant)
                }
                handModel.localScale = Vector3.Lerp(handOrigScale, handTargScale, t); //Interpolate scale between origin and target

                yield return new WaitForFixedUpdate(); //Wait for next fixed update
            }
        }
    }

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (main == null) { main = this; } else { Destroy(this); }    //Singleton-ize this script instance
        handModel = transform.GetChild(0);                            //Get hand model transform (assume it is first child in hierarchy
        handRenderer = GetComponentInChildren<SkinnedMeshRenderer>(); //Get hand renderer
        audioSource = GetComponent<AudioSource>();                    //Get audio source

        //Get starting variables:
        List<Color> handColors = new List<Color>();                                 //Initialize list to store colors of materials in hand
        foreach (Material mat in handRenderer.materials) handColors.Add(mat.color); //Add base color of each material in hand to list
        handMatOrigColors = handColors.ToArray();                                   //Set original hand color array
        handOrigScale = handModel.localScale;                                       //Get original hand model scale
        handTargScale = handOrigScale * hurtFlashScalePercent;                      //Get target scale (since it only needs to be computed once)
    }

    //INSTANCE METHODS:
    /// <summary>
    /// Plays damage animation (based on difference between currentHealth and animHealth).
    /// </summary>
    private void AnimateDamage()
    {
        //Animation procedure:
        //StartCoroutine(HurtFlash());        //Start hurt flash animation
        audioSource.PlayOneShot(hurtSound); //Play hurt sound
        Fingerer.main.DestroyFinger();

        //Cleanup:
        animHealth = currentHealth; //Indicate that health has now been visually updated
        print("Player hit");
    }
    /// <summary>
    /// Plays death animation on player in scene.
    /// </summary>
    private void AnimateDeath()
    {

    }

    //STATIC METHODS:
    /// <summary>
    /// Damages the player by given amount.
    /// </summary>
    /// <param name="damage">Amount of damage dealt to player.</param>
    public static void HurtPlayer(int damage)
    {
        //Do damage animation:
        currentHealth -= damage;             //Subtract damage from player health total
        main.AnimateDamage();                //Play hurt animation on script instance in scene
        if (currentHealth < 0) KillPlayer(); //Kill player if they have been dealt a mortal wound
        FadePlane.Hurt();
    }
    /// <summary>
    /// Triggers player death sequence.
    /// </summary>
    public static void KillPlayer()
    {
        //Player death sequence:
        currentHealth = 5; //Reset health value
        //Send player to the dead island
        SceneManager.LoadScene("deadscene");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlImageLoop : MonoBehaviour
{
    public bool enableLoop = false;
    public Sprite[] AllImages;
    public Image image;
    public float delayEachImage = 3f;

    private int currentIndex = 0;
    public List<int> randomOrder = new List<int>();
    private Coroutine changeImageCoroutine;

    public void enableLooping()
    {
        if(!enableLoop) { 
            GenerateRandomOrder();
            StartChangeImageLoop();
            this.enableLoop = true;
        }
    }

    private void StartChangeImageLoop()
    {
        if (this.changeImageCoroutine != null)
        {
            StopCoroutine(changeImageCoroutine);
        }
        this.changeImageCoroutine = StartCoroutine(ChangeImageLoop());
    }

    public void stopLooping()
    {
        this.enableLoop = false;
        this.randomOrder.Clear();
        if (this.changeImageCoroutine != null)
        {
            StopCoroutine(changeImageCoroutine);
        }
    }

    private void GenerateRandomOrder()
    {
        randomOrder.Clear();
        for (int i = 0; i < AllImages.Length; i++)
        {
            randomOrder.Add(i);
        }

        for (int i = 0; i < AllImages.Length - 1; i++)
        {
            int randomIndex = Random.Range(i, AllImages.Length);
            int temp = randomOrder[randomIndex];
            randomOrder[randomIndex] = randomOrder[i];
            randomOrder[i] = temp;
        }
    }

    private IEnumerator ChangeImageLoop()
    {
        while (true)
        {
            ChangeImage();
            // Wait for some time before changing to the next image
            yield return new WaitForSeconds(this.delayEachImage); // Adjust the duration as needed
        }
    }

    public void ChangeImage()
    {
        if (AllImages.Length == 0)
        {
            Debug.LogError("No images assigned!");
            return;
        }

        int previousIndex = currentIndex;

        if (enableLoop)
        {
            currentIndex = randomOrder[currentIndex];

            currentIndex++;
            if (currentIndex >= AllImages.Length)
            {
                currentIndex = 0;
                GenerateRandomOrder();
            }
        }
        else
        {
            currentIndex++;
            if (currentIndex >= AllImages.Length)
            {
                currentIndex = 0;
            }
        }

        if (currentIndex == previousIndex)
        {
            currentIndex++;
            if (currentIndex >= AllImages.Length)
            {
                currentIndex = 0;
            }
        }

        image.sprite = AllImages[currentIndex];
    }
}

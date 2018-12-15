# Img Share Open source specification

> Thanks for [Windows Community Toolkits](https://github.com/windows-toolkit/WindowsCommunityToolkit)
> The OneDrive service they provide is the core component of the software.

## Function description

`Image Share` is used to upload pictures to OneDrive and then share the pictures to get links that can be accessed directly. Usually used as embedded pictures for web pages, such as blogging.

![AppShot](http://storage.live.com/items/51816931BAB0F7A8!8872?authkey=AO7QXpgYo7-5DUU)

The app is already on the shelf in Microsoft's App Store.

Here is the address: [Microsoft Store](https://www.microsoft.com/store/productId/9NCXNZ52G9Q8)

## Development environment

|Key|Value|
|:-|:-|
|System requirements| Windows10-Ver 1803 or upper|
|Development tool|Visual Studio 2017|
|Programing language|C#|
|Display language|Chinese and English|
|Comment language|Chinese|

## Deployment instructions

> Download the whole project, run `Img_Share.sln`, start the project `Img_Share`, and try to see if it works.

Of course, you can't use **OneDrive** now because you lack `ClientId`.

Because the software relies on **OneDrive**, you have to apply for an **OneDrive** application [here](https://apps.dev.microsoft.com/
) before running the whole project. Remember to add the necessary permissions and place the generated `ClientId` at the location of the following path:

```
OneDriveShareImage > ShareImageTools.cs > _clientId
```

Now, if everything goes well, it should be able to work.

If you log on to OneDrive and can't pop up the permission list, you may not have added Native App under Platform in your **OneDrive App**.

## A hint

1. The UI of the software is not bad, maybe it can give you inspiration.
2. Considering the possibilities of uploading a large number of images, I use Sqlite database as storage. If you don't like it, you can use simple JSON to store data.
3. If you want to add new functions to the software, you are welcome to submit **Pull Request**.
4. I hope you **DON'T** duplicate this app on the **Microsoft Store**.
---

Thank you for your watching. If you have any questions, please send an email [Thansy@foxmail.com](mailto://thansy@foxmail.com) to inquire.


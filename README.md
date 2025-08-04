# OctoStore
OctoStore lets you publish your open source app to the Microsoft Store without leaving your GitHub repo.

Add a single JSON file to your repository and your app will be published to the Microsoft Store, the app store on Windows, free of charge, under the `Open Source Apps on GitHub` publisher.

# How to publish my app to the Microsoft Store

All you need to do is include a file named `ms-store-publish.json` in your repo. Your app can be a Windows executable (`.exe`, `.appx`, or `.appxbundle`, `.msi`, `.msix`, `.msixbundle`) or a [Progressive Web App (PWA)](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/Guides/What_is_a_progressive_web_app).

<details><summary>Windows executables (`.exe`, `.appx`, or `.appxbundle`, `.msi`, `.msix`, `.msixbundle`)</summary>

To publish your Windows app to the Microsoft Store, add a `ms-store-publish.json` to your repo with a `windowsExecutablePackage` section linking to your GitHub Releases executable. [Example ms-store-publish.json](https://github.com/JudahGabriel/ambie/blob/main/ms-store-publish.json):

```json
{
    "name": "Ambie White Noise",
    "iconUrl": "/blob/main/src/AmbientSounds.Uwp/Assets/logo.png?raw=true",
    "category": "HealthFitness",
    "privacyPolicyUrl": "/blob/main/privacypolicy.md",
    "developerEmail": "jenius_apps@outlook.com",
    "windowsExecutablePackage": {
        "gitHubReleasesX64FileName": "Ambie_{{version:4}}_x64.exe",
        "gitHubReleasesArm64FileName": "Ambie_{{version:4}}_ARM64.exe"
    },
    "storeListings": [
        {
            "language": "en",
            "name": "Ambie",
            "description": "Ambie is the ultimate app to help you focus, study, or relax. We use white noise and nature sounds combined with an innovative focus timer to keep you concentrated on doing your best work. It's also fantastic for helping you relax, meditate, and sleep, all of which are essential to keep you refreshed and productive the next day. Altogether, Ambie is a reliable tool to boost your productivity. \r\n\r\n++ One of FastCompany's best new productivity apps in 2022 ++\r\n\r\n### Boost Your Productivity\r\n\r\nTake your productivity into new heights by using our innovative Focus Timer. Configure your focus and rest intervals and let Ambie guide you in your customized Focus Sessions. These sessions are designed to help keep you concentrated on your task, and we'll remind you to take an occasional break to keep your mind fresh.\r\n\r\n### A Growing Catalogue\r\n\r\nAlong with selection of built-in options, Ambie provides a vast catalogue of downloadable sounds both free and premium. Download sounds to add to your collection and discover your next favourite ambience.\r\n\r\n### Premium Subscription\r\n\r\nElevate your productivity by signing up for Ambie+, an affordable premium subscription that provides access to high-quality sounds and videos to help you focus, study, or relax.",
            "shortDescription": "Ambie is an app that plays white noise and nature sounds to help you focus, sleep, and unwind. For many people, having background noise while working on a task helps with concentration. Ambie has a good starting selection of built-in sounds such as rain and beach waves that help you. These can also be used to help you sleep, relax, and de-stress. For instance, those with tinnitus and anxiety have reached out saying Ambie has helped them. And if you download Ambie from the Microsoft Store, you'll get access to a catalogue of online sounds that you can download to expand your library.",
            "screenshots": [
                {
                    "url": "https://store-images.s-microsoft.com/image/apps.39021.14461052683240493.274a6984-fc70-4d2e-998c-34fcbc5f4c8e.2db7cf8a-d066-4ce6-81db-dfdedc96e39a",
                    "caption": "Relax with Ambie's large catalogue of sounds to create your favourite experience"
                },
                {
                    "url": "https://store-images.s-microsoft.com/image/apps.65442.14461052683240493.274a6984-fc70-4d2e-998c-34fcbc5f4c8e.ed08ebf8-9cb4-44f1-9fb4-179b0e33a6ef",
                    "caption": "Use Ambie's built-in Pomodoro timer to help you focus"
                },
                {
                    "url": "https://store-images.s-microsoft.com/image/apps.24658.14461052683240493.274a6984-fc70-4d2e-998c-34fcbc5f4c8e.855a8fb3-8bbe-4e9a-a7c1-f9507ae30cd9",
                    "caption": "Practice selfcare with meditation guides"
                }
            ],
            "keywords": [],
            "developedBy": "Jenius Apps",
            "whatsNew": "We fixed a few issues in channels and focus sessions. We also added official support for fr-CA. Thanks to all of you that sent feedback!"
        }
    ]
}
```

## Releasing a new version of your Windows app to the Microsoft Store

To release a new version of your Windows app to the Microsoft Store, create a new Release on GitHub. The GitHub Release should contain a link the x64 or ARM64 version of your executable, or both. 

For example, you might create a new GitHub Release for your app with the following assets:

- `MyApp_1.0.0_x64.exe`: with a link like https://github.com/MyUser/MyApp/releases/download/v1.0.0.0/MyApp_1.0.0_x64.exe
- `MyApp_1.0.0_ARM64.exe`: with a link like https://github.com/MyUser/MyApp/releases/download/v1.0.0.0/MyApp_1.0.0_ARM64.exe

Then in your `ms-store-publish.json` file, you would have the following:

```json
{
    "windowsExecutablePackage": {
        "gitHubReleasesX64FileName": "MyApp_{{version:3}}_x64.exe",
        "gitHubReleasesArm64FileName": "MyApp_{{version:3}}_ARM64.exe"
    }
}
```

Note that these are file names, not absolute URLs, as OctoStore will automatically grab the latest version of your app from your repo's GitHub Releases.

When adding your executable file name, you can use the `{{version}}` template to specify the version of your app.

- {{version}}: "4.2.0.0"
- {{version:3}}: "4.2.0"
- {{version:2}}: "4.2"

Your executable file should be an `.exe`, `.appx`, `.appxbundle`, `.msi`, `.msix`, or `.msixbundle`.

</details>

<details>

<summary>Progressive Web Apps (PWAs)</summary>

To publish your PWA to the Microsoft Store, add a `ms-store-publish.json` file to repo. It should include a `pwaPackage` section. Here's an [example of such a file](https://github.com/JudahGabriel/etzmitzvot/blob/master/public/ms-store-publish.json). It should look like this:

```json
{
    "name": "EtzMitzvot",
    "iconUrl": "https://etzmitzvot.com/assets/icons/logo-512x512.png", 
    "category": "BooksAndReference",
    "secondaryCategory": "Education",
    "privacyPolicyUrl": "https://etzmitzvot.com/privacy-policy.html",
    "pwaPackage": {
        "url": "https://etzmitzvot.com",
        "manifestUrl": "https://etzmitzvot.com/manifest.json",
        "serviceWorkerUrl": "https://etzmitzvot.com/sw.js"
    },
    "storeListings": [
        {
            "language": "en",
            "name": "Etz Mitzvot",
            "description": "A visual tree graph of the commandments in the Hebrew Bible. Explore commandments, see their relationship to other commandments, understand the Bible better.",
            "shortDescription": "A visual tree graph of the commandments in the Hebrew Bible",
            "screenshots": [
                {
                    "url": "https://etzmitzvot.com/assets/screenshots/screenshot-cmds.png",
                    "caption": "A visual hierarchy of all the commandments in the Hebrew Bible"
                },
                {
                    "url": "https://etzmitzvot.com/assets/screenshots/screenshot-cmd-details.png",
                    "caption": "Tap a commandment to read more about it, see its relationships to other commandments, and read the underlying passage."
                },
                {
                    "url": "https://etzmitzvot.com/assets/screenshots/screenshot-cmd-sidebar.png",
                    "caption": "Dig into statistics and information about the commandments in the Hebrew Bible."
                }
            ],
            "keywords": ["bible", "torah", "judaism", "mitzvot", "commandments"],
            "developedBy": "Bless Israel",
            "features": [
                "Explore the commandments in the Hebrew Bible",
                "See the relationships between commandments",
                "Read the underlying passages of each commandment",
                "Learn about the commandments in a visual way"
            ],
            "whatsNew": "Added commandment 75, added store publish.json"
        }
    ]
}
```

## Required fields for PWAs

For PWAs, `pwaPackage.url` and `pwaPackage.manifestUrl` are required. The `pwaPackage.serviceWorkerUrl` is optional, but recommended.

These URLs must be absolute URLs to your PWA on the web. They cannot be relative URLs or links to files in your repo.

## Releasing a new version of your PWA to the Microsoft Store

To release a new version of your PWA to the Microsoft Store, create a new GitHub Release in your repo.

As with all PWAs, the Microsoft Store will always run the latest version of your PWA on the web. 

Creating a new release on GitHub will cause your app in the Microsoft Store to have an incremented version.

</details>

## Using repo-relative paths in `ms-store-publish.json`

You can use relative paths in your `ms-store-publish.json` file that link to files inside your repo. For example, if your app's icon is stored in your repo at `/public/assets/app-icon.png`, you can link to it like this:

```json
{
    "iconUrl": "/blob/main/public/assets/app-icon.png?raw=true"
}
```

Note that for icons and other binary data, you should use the `?raw=true` query parameter to ensure that the image is served as raw content in order to be fetched by OctoStore.

Icons should be square PNG images, ideally 512x512 pixels or larger.

# Is this a Microsoft-sponsored project?

This started as an individual hackathon idea within Microsoft. It has since been given official support as an experimental idea by the Microsoft Store team. 

Our goal is to see if developers find it useful. Our gauge for determining usefulness is by submitting PRs to add `ms-store-publish.json` to Windows apps repos on GitHub.

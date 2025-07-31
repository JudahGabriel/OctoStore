# OctoStore
OctoStore lets you publish your open source app to the Microsoft Store without leaving your GitHub repo.

Add a single JSON file to your repository and your app will be published to the Microsoft Store, the app store on Windows, free of charge, under the `Open Source Apps on GitHub` publisher.

# How to publish my app to the Microsoft Store

All you need to do is include a file named `ms-store-publish.json` in your repo. Your app can be a Windows executable (.exe), a Windows installer (.msi), or a Progressive Web App (PWA).

<details>

<summary>Progressive Web Apps (PWAs)</summary>

To publish your PWA to the Microsoft Store, add a `ms-store-publish.json` file to your repo. It should look like this:

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

</details>

<details><summary>Windows executables (.exe or .msi)</summary>

Support for exe and msi forthcoming.

</details>

# Is this a Microsoft-sponsored project?

This started as an individual hackathon idea within Microsoft. It has since been given official support as an experimental idea by the Microsoft Store team. 

Our goal is to see if developers find it useful. Our gauge for determining usefulness is by submitting PRs to add `ms-store-publish.json` to Windows apps repos on GitHub.

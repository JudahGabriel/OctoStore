# OctoStore
OctoStore lets you publish your open source app to the Microsoft Store, all without leaving your GitHub repo.

You can think of this as a GitHub Actions build step that publishes an app to the Microsoft Store on Windows.

## How to publish my app to the Microsoft Store

All you need to do is include a file named `ms-store-publish.json` in your repo. For progressive web apps (PWAs), it should look like this:

```json
{
    "name": "EtzMitzvot",
    "iconUrl": "https://etzmitzvot.com/assets/icons/logo-512x512.png",
    "category": "BooksAndReference",
    "secondaryCategory": "Education",
    "privacyPolicyUrl": "https://messianicradio.com/#/privacy",
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

For example, see [this repo's ms-store-publish.json](https://github.com/JudahGabriel/etzmitzvot/blob/master/public/ms-store-publish.json).

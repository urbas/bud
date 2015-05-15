__Preparation__:

- Install the API key (has to be done in administrative console): `choco apiKey -k <your-api-key-here> -source https://chocolatey.org/`

__Release__:

1. Edit release notes.

1. Close Visual Studio and anything that could be locking the files and invoke the following: 

    ```
    bud @performRelease -v x.y.z
    ```

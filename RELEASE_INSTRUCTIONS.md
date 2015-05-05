__Preparation__:

- Install the API key (has to be done in administrative console): `choco apiKey -k <your-api-key-here> -source https://chocolatey.org/`

__Release__:

1. Edit release notes.

1. Invoke the following: 

    ```
    bud @performRelease -v x.y.z
    ```

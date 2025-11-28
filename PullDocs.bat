SET wdk_rev=ac22b06
SET sdk_rev=e192280

IF NOT EXIST "MicrosoftDocs" (

    mkdir MicrosoftDocs
    cd MicrosoftDocs

    git clone --filter=tree:0 --sparse https://github.com/MicrosoftDocs/windows-driver-docs-ddi wdk
    pushd wdk
    git sparse-checkout set wdk-ddi-src/content/dbgeng
    git switch --detach %wdk_rev%

    popd

    git clone --filter=tree:0 --sparse https://github.com/MicrosoftDocs/sdk-api sdk
    pushd sdk
    git sparse-checkout set sdk-api-src/content/winnt
    git switch --detach %sdk_rev%

) ELSE (

    cd MicrosoftDocs

    pushd wdk
    git switch staging
    git pull
    git switch --detach %wdk_rev%
    
    popd
    
    pushd sdk
    git switch docs
    git pull
    git switch --detach %sdk_rev%

)

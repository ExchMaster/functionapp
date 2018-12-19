$sa = Get-AzureRmStorageAccount -ResourceGroupName jaifunctionapp -Name filestorev1
$ctx = $sa.context
$containerName = "samples-workitems"

# upload a file
Set-AzureStorageBlobContent -File "C:\Users\Documents\functionapp\source2.json" `
  -Container $containerName `
  -Blob "source2.json" `
  -Context $ctx `
  -Properties @{"ContentType" = "application/json"} `
  -force

  
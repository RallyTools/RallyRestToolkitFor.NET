## Warranty

The Rally REST API for .NET is available on an as-is basis. 

## Support

Rally Software does not actively maintain this toolkit.  If you have a question or problem, we recommend posting it to Stack Overflow: http://stackoverflow.com/questions/ask?tags=rally 

## Introduction

The Rally REST API for .NET provides an intuitive interface to your Rally Data.  It supports querying items in addition to individual item creates, reads, updates and deletes.  It is compatible with any .NET 4.0 language (C#, VB.NET, F#, etc.)

## Usage

Create a new project in Visual Studio and add a reference to the [Rally.RestApi.dll](https://people.rallydev.com/connector/RallyRestApi/Rally.RestApi-1.0.15.zip) library:

![alt text](https://developer.help.rallydev.com/sites/default/files/multimedia/reference.PNG "Reference")

<p>Set your Project Target Framework to .NET Framework 4:

![alt text](https://developer.help.rallydev.com/sites/default/files/multimedia/vs-targetframework.png "vs-targetframework")

Instantiate a new [RallyRestApi](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_RallyRestApi.htm):

<p><font face =Courier, Courier New, monospace>RallyRestApi restApi = <font color=#0000FF>**new**</font> RallyRestApi(<font color=#0000FF>"myuser@company.com"</font>, <font color=0000FF>"password"</font>,

&nbsp;&nbsp;<font color=#0000FF>"https://rally1.rallydev.com"</font>, <font color=#0000FF>"1.40"</font>);
</font>

<p>The parameters for [RallyRestApi](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_RallyRestApi.htm) are as follows:

<table><tbody>
<tr>
<th>Parameter</th>
<th>Description</th>
<th>Example</th>
</tr>
<tr>
<td>userName*</td>
<td>The username to connect to Rally with.</td>
<td><font face= Courier, Courier New, monospace><font color ="#0000FF">"myuser@company.com"</font></font></td>
</tr>
<tr>
<td>password*</td>
<td>The password to connect to Rally with.</td>
<td><font face= Courier, Courier New, monospace><font color ="#0000FF">"password"</font></font></td>
</tr>
<tr>
<td>server</td>
<td>The Rally server to connect to.

Default is 
[https://rally1.rallydev.com](https://rally1.rallydev.com)

</td>
<td><font face= Courier, Courier New, monospace><font color ="#0000FF">"https://rally1.rallydev.com"</font></font></td>
</tr>
<tr>
<td>WSAPI version</td>
<td>The Web Services API version to use.

Default is 1.24.</td>
<td><font face= Courier, Courier New, monospace><font color ="#0000FF">"1.40"</font></font></td>
</tr>
</tbody></table>
 &nbsp; * = required parameter

## <a name=".NET-PublicMethods"></a>Public Methods

[RallyRestApi](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_RallyRestApi.htm) exposes the following public methods:

<table style="table-layout: fixed"><tbody>
<tr>
<th>Method Name</th>
<th>Parameters</th>
<th>Description</th>
</tr>
<tr>
<td>[Query](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_Query_1_e7c29648.htm)</td>
<td>[Request](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_Request.htm) request*</td>
<td>Search Rally for items matching the specified query. Returns a [QueryResult](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_Response_QueryResult.htm) object containing the results of the request.</td>
</tr></tbody>

<table><tbody>
<tr>
<td>**Example:**
<font face=Courier, Courier new, monospace><font color =#009900>//Build request</font>

Request request = <font color=#0000FF>**new**</font> Request(<font color=#0000FF>"defect"</font>);

request.Fetch = <font color=#0000FF>**new**</font> List&lt;string&gt;()

&nbsp;&nbsp;&nbsp;&nbsp;{

&nbsp;&nbsp;&nbsp;&nbsp;<font color=#0000FF>"Name"</font>,

&nbsp;&nbsp;&nbsp;&nbsp;<font color=#0000FF>"Description"</font>,

&nbsp;&nbsp;&nbsp;&nbsp;<font color=#0000FF>"FormattedID"</font>,

&nbsp;&nbsp;&nbsp;&nbsp;};

request.Query = <font color=#0000FF>**new**</font> Query(<font color=#0000FF>"Name"</font>, Query.Operator.Equals, <font color=#0000FF>"My Defect"</font>)

.And(<font color=#0000FF>**new**</font> Query(<font color=#0000FF>"State"</font>, Query.Operator.Equals, <font color=#0000FF>"Submitted"</font>));

<font color =#009900>//Make request and process results</font>

QueryResult queryResult = restApi.Query(request) ; foreach(var result in queryResult.Results) {

string itemName = result[<font color =#0000FF>"Name"</font>];

}</font></td>
</tr>
<tr>
<td>**Example:**
<font face=Courier, Courier new, monospace><font color =#009900>//Build Portfolio Item Request</font>

Request request = <font color=#0000FF>**new**</font> Request(<font color=#0000FF>"PortfolioItem/Initiative"</font>);

request.Fetch = <font color=#0000FF>**new**</font> List&lt;string&gt;()

&nbsp;&nbsp;&nbsp;&nbsp;{

&nbsp;&nbsp;&nbsp;&nbsp;<font color=#0000FF>"Name"</font>,

&nbsp;&nbsp;&nbsp;&nbsp;<font color=#0000FF>"Description"</font>,

&nbsp;&nbsp;&nbsp;&nbsp;<font color=#0000FF>"FormattedID"</font>,

&nbsp;&nbsp;&nbsp;&nbsp;};

</td>
</tr>
</tbody>

<table style="table-layout: fixed">
<tbody>
<tr>
<td>[GetByReference](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_GetByReference_2_f5a7332b.htm)</td>
<td>string ref*, params string[] fetchFields</td>
<td>Retrieve the Rally object represented by the specified ref. If fetchFields is specified those fields will be included in the returned object. Returns the resulting [DynamicJsonObject](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_DynamicJsonObject.htm).</td>
</tr></tbody>

<table>
<tbody>
<tr>
<td>**Example:**
<font face=Courier, Courier new, monospace>DynamicJsonObject item = 

restApi.GetByReference

(<font color =#0000FF>"https://preview.rallydev.com/slm/webservice/1.40/defect/12345.js"</font>, <font color =#0000FF>"Name"</font>,  <font color =#0000FF>"FormattedID"</font>);

string itemName = item[<font color =#0000FF>"Name"</font>];</font></td>
</tr>

<table style="table-layout: fixed"><tbody>
<tr>
<td>[GetByReference](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_GetByReference_3_f5cd4999.htm)</td>
<td>string type*, long oid*, params string[] fetchFields</td>
<td>Overload of GetByReference using type and object id instead of a reference. If fetchFields is specified those fields will be included in the returned object. Returns the resulting [DynamicJsonObject](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_DynamicJsonObject.htm).</td></tr></tbody>

<table><tbody>
<tr>
<td>**Example:**

<font face=Courier, Courier new, monospace>DynamicJsonObject item = 

restApi.GetByReference(<font color=#0000FF>"defect"</font>, <font color=#009900>12345</font>, <font color=#0000FF>"Name"</font>, <font color=#0000FF>"FormattedID"</font>);

string itemName = item[<font color=#0000FF>"Name"</font>];</font></td>
</tr></tbody>

<table style="table-layout: fixed"><tbody>
<tr>
<td>[Create](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_Create_2_39c5ba29.htm)</td>
<td>string reference*, string type*, [DynamicJsonObject](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_DynamicJsonObject.htm) object*</td>
<td>Create an object of the specified type with the specified data in Rally.

Returns a [CreateResult](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_Response_CreateResult.htm) object with the results of the request. </td></tr></tbody>

<table><tbody>
<tr>
<td>**Example:**

<font face=Courier, Courier new, monospace>
String workspaceRef = "/workspace/12345678910";

DynamicJsonObject toCreate = <font color=#0000FF>**new**</font> DynamicJsonObject(); 

toCreate[<font color=#0000FF>"Name"</font>] = <font color=#0000FF>"My Defect"</font>;

CreateResult createResult = restApi.Create(workspaceRef,<font color=#0000FF>"defect"</font>, toCreate);</font></td>
</tr></tbody>

<table style="table-layout: fixed"><tbody>
<tr>
<td>[Update](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_Update_2_39c5ba29.htm)</td>
<td>string reference*, [DynamicJsonObject](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_DynamicJsonObject.htm) data*</td>
<td>Update the specified item in Rally.

Returns an [OperationResult](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_Response_OperationResult.htm) object with the results of the request.</td>
</tr></tbody>

<table><tbody>
<tr>
<td>**Example:**

<font face=Courier, Courier new, monospace>DynamicJsonObject toUpdate = <font color=#0000FF>**new**</font> DynamicJsonObject(); 

toUpdate[<font color=#0000FF>"Description"</font>] = <font color=#0000FF>"This is my defect."</font>;

OperationResult updateResult = restApi.Update (<font color=#0000FF>"https://preview.rallydev.com/slm/webservice/1.40/defect/12345.js"</font>,

toUpdate);</font></td>
</tr></tbody>

<table style="table-layout: fixed"><tbody>
<tr>
<td>[Update](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_Update_3_024c7b0f.htm)</td>
<td>string type*, long oid*, [DynamicJsonObject](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_DynamicJsonObject.htm) data*</td>
<td>Overload of Update using type and object id instead of a reference. Update the specified item in Rally.

Returns an [OperationResult](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_Response_OperationResult.htm) object with the results of the request.</td>
</tr></tbody>

<table><tbody>
<td>**Example:**

<font face=Courier, Courier new, monospace>DynamicJsonObject toUpdate = <font color=#0000FF>**new**</font> DynamicJsonObject();

toUpdate[<font color =#0000FF>"Description"</font>] = <font color=#0000FF>"This is my defect."</font>;
 OperationResult updateResult = restApi.Update(<font color=#0000FF>"defect"</font>, 12345L, toUpdate);</font></td>
</tr></tbody>

<table style="table-layout: fixed">
<tbody>
<tr>
<td>[Delete](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_Delete_1_bb3a7a4f.htm)</td>
<td>string reference*, string reference*</td>
<td>Delete the specified object in Rally.

Returns an [OperationResult](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_Response_OperationResult.htm) object with the results of the request.</td>
</tbody>

<table><tbody>
<tr>
<td>**Example:**

<font face=Courier, Courier new, monospace>
String workspaceRef=<font color=#0000FF>"/workspace/12345678910"</font>;

String objectRef=<font color=#0000FF>"/defect/12345678912";</font>

OperationResult deleteResult = restApi.Delete (workspaceRef, objectRef);</font></td>
</tr></tbody>

<table style="table-layout: fixed"><tbody>
<tr>
<td>[Delete](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_Delete_2_a98329ad.htm)</td>
<td>string reference*, string type*, long oid*</td>
<td>Overload of Delete using type and object id instead of a reference. Delete the specified object in Rally.

Returns an [OperationResult](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_Response_OperationResult.htm) object with the results of the request.</td>
</tr></tbody>

<table><tbody>
<td>**Example:**

<font face=Courier, Courier new, monospace>
String workspaceRef=<font color=#0000FF>"/workspace/12345678910"</font>;

Long objectID=<font color=#0000FF>12345678912L;</font>

String itemType=<font color=#0000FF>"Defect";</font>

OperationResult deleteResult = restApi.Delete (workspaceRef, itemType objectID);</font></td>;</font></td>
</tr>
</tbody>

<table style="table-layout: fixed"><tbody>
<tr>
<td>[GetAllowedAttributeValues](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_GetAllowedAttributeValues_2_f3570541.htm)</td>
<td>string type*, string attribute*</td>
<td>Returns a [DynamicJsonObject](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_DynamicJsonObject.htm) containing the allowed values for the specified type and attribute.</td></tr></tbody>

<table><tbody>
<td>**Example:**

<font face=Courier, Courier new, monospace>DynamicJsonObject allowedValues = restApi.GetAllowedAttributeValues(<font color=#0000FF>"defect"</font>, <font color=#0000FF>"severity"</font>);</font></td>
</tr></tbody>

<table style="table-layout: fixed"><tbody>
<tr>
<td>[GetAttributesByType](https://docs.rallydev.com/restapinet/html/M_Rally_RestApi_RallyRestApi_GetAttributesByType_1_bb3a7a4f.htm)</td>
<td>string typeName*</td>
<td>Returns a [QueryResult](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_Response_QueryResult.htm) object containing the attribute definitions for the specified type.

**Note:** typeName should be the Name of the type- not the ElementName.

Example: "Hierarchical Requirement" instead of "hierarchicalrequirement".</td>
</tr></tbody>

<table><tbody>
<tr>
<td>**Example:**

<font face=Courier, Courier new, monospace>QueryResult attributeDefs = restApi.GetAttributesByType(<font color=#0000FF>"Defect"</font>);</font></td>
</tr>
</tbody></table>

 &nbsp; * = required parameter</div>

### <a name=".NET-Logging"></a>Logging

The Rally REST API for .NET provides the ability to log all requests and responses to aid in troubleshooting.

To enable this behavior simply configure one or more trace listeners at the Information level.

Below is an example App.config which will enable this:

```
{
    configuration:{
        system:{
            value:undefined,
            diagnostics:{
                trace:{
                    autoflush:'true',
                    indentsize:4,
                    listeners:{
                        add:{
                            name:'requestResponseLogger"\ntype="System.Diagnostics.TextWriterTraceListener',
                            initializedata:'RallyRestApi.log',
                            filter:{
                                type:'System.Diagnostics.EventTypeFilter',
                                initializedata:'Information'
                            }
                        },
                        remove:{
                            name:'Default'
                        }
                    }
                }
            }
        }
    }
}
```

### <a name=".NET-Example"></a>Example

The following code illustrates how to create, update, read, query and delete using the [RallyRestApi](https://docs.rallydev.com/restapinet/html/T_Rally_RestApi_RallyRestApi.htm) object.

```
//Initialize the REST API
RallyRestApi restApi = new RallyRestApi("username", "password", "https://rally1.rallydev.com", "1.40");

//Create an item
DynamicJsonObject toCreate = new DynamicJsonObject();
toCreate["Name"] = "My Defect";
CreateResult createResult = restApi.Create("defect", toCreate);

//Update the item DynamicJsonObject toUpdate = new DynamicJsonObject();
toUpdate["Description"] = "This is my defect.";
OperationResult updateResult = restApi.Update(createResult.Reference, toUpdate);

//Get the item
DynamicJsonObject item = restApi.GetByReference(createResult.Reference, "Name");
string name = item["Name"];

//Query for items
Request request = new Request("defect");
request.Fetch = new List()
    {
    "Name",
    "Description",
    "FormattedID"
     };
request.Query = new Query("Name", Query.Operator.Equals, "My Defect");
QueryResult queryResult = restApi.Query(request);
foreach(var result in queryResult.Results)
{
  //Process item
  string formattedID = result["FormattedID"];
}

//Delete the item
OperationResult deleteResult = restApi.Delete(createResult.Reference);
```

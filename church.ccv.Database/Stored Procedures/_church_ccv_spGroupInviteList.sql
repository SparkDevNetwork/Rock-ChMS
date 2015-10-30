/*
<doc>
	<summary>
 		This stored procedure returns the invite list for the group provided.
	</summary>

	<returns></returns>
	<param name="GroupId" datatype="int">The group id to use for the invite list.</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spGroupInviteList] 1202481
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[_church_ccv_spGroupInviteList]
	@GroupId int 

AS

BEGIN
	
	SELECT 
		fp.[Id],
		fp.[LastName] + ', ' + fp.[NickName] AS [Name],
		fp.[Email],
		(SELECT [Value] FROM [DefinedValue] dv WHERE dv.[Id] = fp.[ConnectionStatusValueId]) AS [ConnectionStatus],
		(SELECT [NumberFormatted] FROM [PhoneNumber] p WHERE p.[PersonId] = fp.[Id] AND p.[NumberTypeValueId] = 13) AS [HomePhone],
		(SELECT [NumberFormatted] FROM [PhoneNumber] p WHERE p.[PersonId] = fp.[Id] AND p.[NumberTypeValueId] = 12) AS [CellPhone],
		(SELECT CONVERT(datetime, [ValueAsDateTime], 101) FROM [AttributeValue] av WHERE av.[AttributeId] = 174 AND av.[EntityId] = fp.[Id] AND av.[ValueAsDateTime] != '1900-01-01') AS [BaptismDate],
		(SELECT CONVERT(datetime, [ValueAsDateTime], 101) FROM [AttributeValue] av WHERE av.[AttributeId] = 1309 AND av.[EntityId] = fp.[Id] AND av.[ValueAsDateTime] != '1900-01-01') AS [StartingPointDate],
		(SELECT [Street1] FROM [Location] l WHERE l.Id = fgl.[LocationId]) + ' ' +
		(SELECT [Street2] FROM [Location] l WHERE l.Id = fgl.[LocationId]) + '<br>' +
		(SELECT [City] FROM [Location] l WHERE l.Id = fgl.[LocationId]) + ', ' +
		(SELECT [State] FROM [Location] l WHERE l.Id = fgl.[LocationId]) + ' ' +
		(SELECT [PostalCode] FROM [Location] l WHERE l.Id = fgl.[LocationId]) AS [Address],
		(ROUND(ng.[Distance] / 1609.344,2)) AS [Distance]
	FROM [_church_ccv_Datamart_NearestGroup] ng
		INNER JOIN [GroupLocation] fgl ON fgl.[LocationId] = ng.[FamilyLocationId] AND fgl.[IsMappedLocation] = 1
		INNER JOIN [Group] fg ON fg.id = fgl.[GroupId] and fg.[GroupTypeId] = 10
		INNER JOIN [GroupMember] fgm ON fgm.[GroupId] = fg.[Id] AND fgm.[GroupRoleId] = 3
		INNER JOIN [Person] fp ON fp.[Id] = fgm.[PersonId] AND fp.[ConnectionStatusValueId] IN (65,146) AND fp.[RecordStatusValueId] = 3
		INNER JOIN [GroupLocation] sgl ON sgl.[LocationId] = ng.[GroupLocationId]
		INNER JOIN [Group] sg ON sg.[Id] = sgl.[GroupId] and sg.[GroupTypeId] = 49
	WHERE sg.[Id] = @GroupId 
		AND fp.[Id] NOT IN (SELECT gp.[Id] 
								FROM [Person] gp 
									INNER JOIN [GroupMember] ggm ON ggm.[PersonId] = gp.[Id]
									INNER JOIN [Group] gg ON gg.[Id] = ggm.[GroupId]
								WHERE
									gg.[GroupTypeId] = 49)
	ORDER BY fp.[LastName],fp.[NickName]
END


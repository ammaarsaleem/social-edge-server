using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SocialEdge.Server.DataService
{
    public interface ICollection
    {
        ///<summary>Returns an estimate of the number of documents in the collection.</summary>      
        ///<returns>An estimate of the number of documents in the collection.</returns>        
        long EstimatedDocumentCount{ get;}
        ///<summary>Counts the number of documents in the collection. For a fast estimate of the
        //     total documents in a collection use EstimatedDocumentCount</summary>      
        ///<returns>The number of documents in the collection.</returns>
        long DocumentCount{ get;}
             
        ///<summary>Finds the document matching the id.</summary>      
        ///<param name="id">id of the document to match. </param>
        ///<returns>Bson document.</returns>
        Task<BsonDocument> FindOneById(string id);

        ///<summary>Finds a document matching the the value of the property. Incase of multiple matches, fetches the first match</summary>      
        ///<param name="prop">The property to match</param>
        ///<param name="val">The value of the property to match</param>
        ///<returns>Bson document.</returns>          
        Task<BsonDocument> FindOne<T>(string prop, T val);
        ///<summary>Finds document matching the filter.</summary>      
        ///<param name="filter">FilterDefinition</param>
        ///<returns>Bson document.</returns>   
        Task<BsonDocument> FindOne(FilterDefinition<BsonDocument> filter);
        ///<summary>Finds documents matching the the value of the property.</summary>      
        ///<param name="prop">The property to match</param>
        ///<param name="val">The value of the property to match</param>
        ///<returns>List of Bson documents</returns>  
        Task<List<BsonDocument>> Find<T>(string prop,T val);
        ///<summary>Finds documents matching the filter.</summary>      
        ///<param name="filter">FilterDefinition</param>
        ///<returns>List of Bson documents</returns>   
        Task<List<BsonDocument>> Find(FilterDefinition<BsonDocument> Filter);
        ///<summary>Updates the matching document</summary>      
        ///<param name="id">Id of the document to update</param>
        ///<param name="prop">The property to update</param>
        ///<param name="val">New value of prop</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>           
        Task<UpdateResult> UpdateOneById<T>(string id, string prop, T val, bool upsert=false);
        ///<summary>Updates the matching document</summary>      
        ///<param name="UpdateDefinition">Update definition</param>
        ///<returns>UpdateResult</returns>           
        Task<UpdateResult> UpdateOneById(string id, UpdateDefinition<BsonDocument> UpdateDefinition, bool upsert=false);
        ///<summary>Updates the first matching document</summary>      
        ///<param name="filterProp">Property to match</param>
        ///<param name="filterVal">Value of property in filterProp to match</param>
        ///<param name="updateProp">The property to update</param>
        ///<param name="updateVal">New value of updateProp</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>             
        Task<UpdateResult> UpdateOne<T,V>(string filterProp, T filterVal, string updateProp, V updateVal, bool upsert=false);
       ///<summary>Updates the first matching document</summary>      
        ///<param name="filterProp">Property to match</param>
        ///<param name="filterVal">Value of property in filterProp to match</param>
        ///<param name="updateDefinition">Update definition</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>            
        Task<UpdateResult> UpdateOne<T>(string filterProp, T filterVal, UpdateDefinition<BsonDocument> updateDefinition, bool upsert=false);
       ///<summary>Updates the first matching document</summary>      
        ///<param name="filter">Filter definition</param>
        ///<param name="updateDefinition">Update definition</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>   
        Task<UpdateResult> UpdateOne(FilterDefinition<BsonDocument> filter,UpdateDefinition<BsonDocument> updateDefinition, bool upsert=false);
        ///<summary>Updates the first matching document</summary>      
        ///<param name="prop">Property to match</param>
        ///<param name="val">Value of property in filterProp to match</param>
        ///<param name="updateDefinition">Update definition</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>            
        Task<UpdateResult> UpdateMany<T>(string prop,T val, UpdateDefinition<BsonDocument> updateDefinition, bool upsert=false);
        ///<summary>Updates all matching documents</summary>      
        ///<param name="filterProp">Property to match</param>
        ///<param name="filterVal">Value of property in filterProp to match</param>
        ///<param name="updateProp">The property to update</param>
        ///<param name="updateVal">New value of updateProp</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>               
        Task<UpdateResult> UpdateMany<T,V>(string filterProp, T filterVal, string updateProp, V updateVal, bool upsert=false);
        ///<summary>Updates all matching documents</summary>      
        ///<param name="filter">Filter definition</param>
        ///<param name="updateDefinition">Update definition</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>          
        Task<UpdateResult> UpdateMany(FilterDefinition<BsonDocument> filter,UpdateDefinition<BsonDocument> updateDefinition, bool upsert=false);
        ///<summary>Removes the document that matches the if</summary>      
        ///<param name="id">id of the document to remove</param>
        ///<returns>Bool indicating operation was successful or not</returns> 
        Task<bool> RemoveOneById(string id);
        ///<summary>Removes the document that matches the value of given property</summary>      
        ///<param name="prop">Property to match</param>
        ///<param name="val">Value of the property to match</param>
        ///<returns>Bool indicating operation was successful or not</returns>         
        Task<bool> RemoveOne<T>(string prop, T val);
        ///<summary>Removes the document that matches the filter</summary>      
        ///<param name="filter">Filter definition</param>
        ///<returns>Bool indicating operation was successful or not</returns>  
        Task<bool> RemoveOne(FilterDefinition<BsonDocument> filter);
        ///<summary>Removes documents which match the value of given property</summary>      
        ///<param name="prop">Property to match</param>
        ///<param name="val">Value of the property to match</param>
        ///<returns>Bool indicating operation was successful or not</returns>           
        Task<long> RemoveMany<T>(string prop, T val);
        ///<summary>Removes documents which match the filter</summary>      
        ///<param name="filter">Filter definition</param>
        ///<returns>Bool indicating operation was successful or not</returns>          
        Task<long> RemoveMany(FilterDefinition<BsonDocument> Filter);
        ///<summary>Inserts a document</summary>      
        ///<param name="document">Bson document to insert</param>
        ///<returns>Bool indicating operation was successful or not</returns>         
        Task<bool> InsertOne(BsonDocument document);
        ///<summary>Inserts multiple document</summary>      
        ///<param name="document">List of Bson documents to insert</param>
        ///<returns>Bool indicating operation was successful or not</returns>              
        Task<bool> InsertMany(List<BsonDocument> documents);
        // Task<bool> Save(BsonDocument document);
        ///<summary>Creates an index</summary>      
        ///<param name="key">the field to create index on</param>
        ///<param name="direction">1 for ascending, -1 for descending</param>
        ///<param name="unique">Should the index be unique or not</param>
        ///<param name="name">Name of the index</param>
        ///<param name="TTL">Time To Live for the index</param>        
        ///<returns>Name of the index that was created.</returns>           
        Task<string> CreateIndex(string key, int direction=1,  bool unique=false, 
                                string name=null, TimeSpan? TTL=null);
        ///<summary>Creates a multifield index</summary>      
        ///<param name="key1">the first field to create index on</param>
        ///<param name="direction1">1 for ascending, -1 for descending</param>
        ///<param name="key2">the secondd to create index on</param>
        ///<param name="direction2">1 for ascending, -1 for descending</param>        
        ///<param name="unique">Should the index be unique or not</param>
        ///<param name="name">Name of the index</param>
        ///<param name="TTL">Time To Live for the index</param>        
        ///<returns>Name of the index that was created.</returns>  
        Task<string> CreateIndex(string key1, string key2,  int direction1=1, int direction2=1,
                                bool unique=false,string name=null, TimeSpan? TTL=null);




    }
}
using System;
using Xunit;
using SocialEdge.Server.DataService;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tests
{
    public class CollectionTests
    {
        IDataService _dataService;
        ICollection collection;
        public CollectionTests(IDataService dataService)
        {
            _dataService=dataService;
            collection = _dataService.GetCollection("BooksTest");
        }
        
        [Theory]
        [InlineData("1")]
        public async void FindOneById_WhenMatchExists_IdShouldMatch(string id)
        {
            var result =await collection.FindOneById(id);
            var _id = result["_id"].ToString();
            Assert.True(id==_id);
        }

        [Theory]
        [InlineData("-31")]
        public async void FindOneById_WhenDocNotExists_ShouldReturnNull(string id)
        {
            var result =await collection.FindOneById(id);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("price",10)]
        public async void FindOneT_WhenMatchExists_PropertyShouldMatch(string prop, int val)
        {
            var result = await collection.FindOne<int>(prop,val);
            Assert.Equal(result[prop],val);
        }

        [Theory]
        [InlineData("price",-1)]
        public async void FindOneT_WhenDocNotExists_ShouldReturnNull(string prop,int val)
        {
            var result = await collection.FindOne<int>(prop,val);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("1","price",10,false)]
        public async void UpdateOneById_WhenMatchExists_ShouldReturnModifiedCountOne(string id,string updateProp,int updateVal,bool upsert)
        {
            var result = await collection.UpdateOneById<int>(id,updateProp,updateVal,upsert);
            Assert.Equal(result.ModifiedCount,1);
        }

        [Theory]
        [InlineData("23","price",10,false)]
        public async void UpdateOneById_WhenMatchNotExistsAndUpsertFalse_ShouldReturnUpsertIdEmpty(string id,string updateProp,int updateVal,bool upsert)
        {
            var result = await collection.UpdateOneById<int>(id,updateProp,updateVal,upsert);
            Assert.Equal(result.UpsertedId,string.Empty); 
        }        

        [Theory]
        [InlineData("23","price",10,true)]
        public async void UpdateOneById_WhenMatchNotExistsAndUpsertTrue_ShouldReturnUpsertedId(string id,string updateProp,int updateVal,bool upsert)
        {
            var result = await collection.UpdateOneById<int>(id,updateProp,updateVal,upsert);
            Assert.NotEqual(result.UpsertedId,string.Empty); 
        }        


        public static IEnumerable<object[]> WhenMultipleMatchingDocExists()
        {
            var updateDefinition = Builders<BsonDocument>.Update.Set("name","new name");
            yield return new object[] { "price",10,updateDefinition };
        }

        public static IEnumerable<object[]> WhenOneMatchingDocExists()
        {
            var updateDefinition = Builders<BsonDocument>.Update.Set("name","new name");
            yield return new object[] { "price",20,updateDefinition };
        }

        public static IEnumerable<object[]> WhenNoMatchingDocExists()
        {
            var updateDefinition = Builders<BsonDocument>.Update.Set("name","new name");
            yield return new object[] { "price",99,updateDefinition };
        }

        [Theory]
        [MemberData(nameof(WhenMultipleMatchingDocExists))]
        public async void UpdateOneT_WhenMultipleMatchingDocExists_ShouldReturnModifiedCountOne(string prop, int val, 
                                                                                                UpdateDefinition<BsonDocument> updateDefinition)
        {
            var result = await collection.UpdateOne<int>(prop,val,updateDefinition);
            Assert.Equal(result.ModifiedCount,1);
        }        
        
        [Theory]
        [MemberData(nameof(WhenOneMatchingDocExists))]
        public async void UpdateOneT_WhenOneMatchExists_ShouldReturnModifiedCountOne(string prop, int val, UpdateDefinition<BsonDocument> updateDefinition)
        {
            var result = await collection.UpdateOne<int>(prop,val,updateDefinition);
            Assert.Equal(result.ModifiedCount,1);
        }

        [Theory]
        [MemberData(nameof(WhenNoMatchingDocExists))]
        public async void UpdateOneT_WhenNoMatchExistsAndUpsertTrue_ShouldReturnUpsertedId(string prop, int val, UpdateDefinition<BsonDocument> updateDefinition)
        {
            var result = await collection.UpdateOne<int>(prop,val,updateDefinition);
            Assert.NotEqual(result.UpsertedId,string.Empty);
        }
        
        [Theory]
        [MemberData(nameof(WhenNoMatchingDocExists))]
        public async void UpdateOneT_WhenNoMatchExistsAndUpsertFalse_ShouldReturnUpsertedId(string prop, int val, UpdateDefinition<BsonDocument> updateDefinition)
        {
            var result = await collection.UpdateOne<int>(prop,val,updateDefinition);
            Assert.Equal(result.UpsertedId,string.Empty);
        }

        [Theory]
        [InlineData("price",10,"quantity","10 Amendment")]
        public async void UpdateOneTV_WhenMultipleMatchingDocExists_ShouldReturnModifiedCountOne(string filterProp, int filterVal, 
                                                                                                string updateProp, string updateVal)
        {
            var result = await collection.UpdateOne<int,string>(filterProp,filterVal,updateProp,updateVal);
            Assert.Equal(result.ModifiedCount,1);
        }               
        
        [Theory]
        [InlineData("price",20,"quantity","20 Amendment")]
        public async void UpdateOneTV_WhenOneMatchExists_ShouldReturnModifiedCountOne(string filterProp, int filterVal, 
                                                                                    string updateProp, string updateVal)
        {
            var result = await collection.UpdateOne<int,string>(filterProp,filterVal,updateProp,updateVal);
            Assert.Equal(result.ModifiedCount,1);
        }                   

        [Theory]
        [InlineData("price",99,"quantity","99 Amendment",false)]
        public async void UpdateOneTV_WhenNoMatchingDocExistsAndUpsertTrue_ShouldReturnUpsertId(string filterProp, int filterVal, 
                                                                                                string updateProp, string updateVal, bool upsert)
        {
            var result = await collection.UpdateOne<int,string>(filterProp,filterVal,updateProp,updateVal,upsert);
            Assert.NotEqual(result.UpsertedId,string.Empty); 
        } 

        [Theory]
        [InlineData("price",99,"quantity","99 Amendment",false)]
        public async void UpdateOneTV_WhenNoMatchingDocExistsAndUpsertFalse_ShouldReturnUpsertIdEmpty(string filterProp, int filterVal, 
                                                                                                string updateProp, string updateVal, bool upsert)
        {
            var result = await collection.UpdateOne<int,string>(filterProp,filterVal,updateProp,updateVal,upsert);
            Assert.Equal(result.UpsertedId,string.Empty); 
        } 

        [Theory]
        [InlineData("price",10,"quantity","10 Amendment")]
        public async void UpdateManyTV_WhenMultipleMatchingDocExists_ShouldReturnModifiedCountGreaterThanOne(string filterProp, int filterVal, 
                                                                                                string updateProp, string updateVal)
        {
            var result = await collection.UpdateMany<int,string>(filterProp,filterVal,updateProp,updateVal);
            Assert.True(result.ModifiedCount>1);
        }               
        
        [Theory]
        [InlineData("price",20,"quantity","20 Amendment")]
        public async void UpdateManyTV_WhenOneMatchExists_ShouldReturnModifiedCountOne(string filterProp, int filterVal, 
                                                                                    string updateProp, string updateVal)
        {
            var result = await collection.UpdateMany<int,string>(filterProp,filterVal,updateProp,updateVal);
            Assert.Equal(result.ModifiedCount,1);
        }                   

        [Theory]
        [InlineData("price",99,"quantity","99 Amendment",true)]
        public async void UpdateManyTV_WhenNoMatchingDocExistsAndUpsertTrue_ShouldReturnUpsertId(string filterProp, int filterVal, 
                                                                                                string updateProp, string updateVal,
                                                                                                bool upsert)
        {
            var result = await collection.UpdateMany<int,string>(filterProp,filterVal,updateProp,updateVal,upsert);
            Assert.NotEqual(result.UpsertedId,string.Empty); 
        }   

        [Theory]
        [InlineData("price",99,"quantity","99 Amendment")]
        public async void UpdateManyTV_WhenNoMatchingDocExistsAndUpsertFalse_ShouldReturnUpsertIdEmpty(string filterProp, int filterVal, 
                                                                                                string updateProp, string updateVal)
        {
            var result = await collection.UpdateMany<int,string>(filterProp,filterVal,updateProp,updateVal);
            Assert.Equal(result.UpsertedId,string.Empty); 
        } 


        [Theory]
        [MemberData(nameof(WhenMultipleMatchingDocExists))]
        public async void UpdateManyT_WhenMultipleMatchingDocExists_ShouldReturnModifiedCountGreaterThanOne(string filterProp, int filterVal, 
                                                                                                UpdateDefinition<BsonDocument> updateDefinition)
        {
            var result = await collection.UpdateMany<int>(filterProp,filterVal,updateDefinition);
            Assert.True(result.ModifiedCount>1);
        }               
        
        [Theory]
        [MemberData(nameof(WhenOneMatchingDocExists))]
        public async void UpdateManyT_WhenOneMatchExists_ShouldReturnModifiedCountOne(string filterProp, int filterVal, 
                                                                                    UpdateDefinition<BsonDocument> updateDefinition)
        {
            var result = await collection.UpdateMany<int>(filterProp,filterVal,updateDefinition);
            Assert.Equal(result.ModifiedCount,1);
        }                   

        [Theory]
        [MemberData(nameof(WhenNoMatchingDocExists))]
        public async void UpdateManyT_WhenNoMatchingDocExistsAndUpsertIsTrue_ShouldReturnUpsertId(string filterProp, int filterVal, 
                                                                                    UpdateDefinition<BsonDocument> updateDefinition, bool upsert)
        {
            var result = await collection.UpdateMany<int>(filterProp,filterVal,updateDefinition, upsert);
            Assert.NotEqual(result.UpsertedId,string.Empty); 
        }    

        [Theory]
        [MemberData(nameof(WhenNoMatchingDocExists))]
        public async void UpdateManyT_WhenNoMatchingDocExistsAndUpsertIsFalse_ShouldReturnUpsertIdEmpty(string filterProp, int filterVal, 
                                                                                    UpdateDefinition<BsonDocument> updateDefinition, bool upsert)
        {
            var result = await collection.UpdateMany<int>(filterProp,filterVal,updateDefinition, upsert);
            Assert.Equal(result.UpsertedId,string.Empty); 
        }      

        [Theory]
        [InlineData("99")]
        public async void RemoveOneById_WhenMatchExists_ShouldReturnTrue(string id)
        {
            var result = await collection.RemoveOneById(id);
            Assert.True(result);
        }        

        [Theory]
        [InlineData("120")]
        public async void RemoveOneById_WhenNoMatchExists_ShouldReturnFalse(string id)
        {
            var result = await collection.RemoveOneById(id);
            Assert.False(result);
        }             

        //Integration
        [Theory]
        [InlineData("99")]
        public async void RemoveOneById_WhenMatchExists_ShouldRemoveOne(string id)
        {
            var removeResult = await collection.RemoveOneById(id);
            var findResult = await collection.FindOneById(id);
            Assert.Equal(findResult,new BsonDocument());

        }        

        [Theory]
        [InlineData("price",10)]
        public async void RemoveOneByT_WhenMatchExists_ShouldReturnTrue(string filterProp, int filterVal)
        {
            var result = await collection.RemoveOne<int>(filterProp, filterVal);
            Assert.True(result);            
        }        

        [Theory]
        [InlineData("price",123)]
        public async void RemoveOneByT_WhenNoMatchExists_ShouldReturnFalse(string filterProp, int filterVal)
        {
            var result = await collection.RemoveOne<int>(filterProp, filterVal);
            Assert.True(result);  
        }        

        [Theory]
        [InlineData("price",10)]
        public async void RemoveOneByT_WhenMultipleMatchExists_ShouldReturnTrue(string filterProp, int filterVal)
        {
            var result = await collection.RemoveOne<int>(filterProp, filterVal);
            Assert.True(result);  
        }        

        //Integration
        [Theory]
        [InlineData("price",20)]
        public async void RemoveOneByT_WhenMatchExists_ShouldRemoveOne(string filterProp, int filterVal)
        {
            var removeResult = await collection.RemoveOne<int>(filterProp,filterVal);
            var findResult = await collection.FindOne<int>(filterProp,filterVal);
            Assert.Equal(findResult,new BsonDocument());
        }    

        [Theory]
        [InlineData("quantity",5)]
        public async void RemoveManyT_WhenMultipleMatchExists_ShouldReturnGreaterThanZero(string filterProp, int filterVal)
        {
            var result = await collection.RemoveMany<int>(filterProp, filterVal);
            Assert.True(result>0);  
        }          

        [Theory]
        [InlineData("quantity",1)]
        public async void RemoveManyT_WhenOneMatchExists_ShouldReturnOne(string filterProp, int filterVal)
        {
            var result = await collection.RemoveMany<int>(filterProp, filterVal);
            Assert.True(result==1);  
        }          

        [Theory]
        [InlineData("category")]
        public async void CreateIndex_WhenCreatedWithoutName_ShouldReturnIndexName(string key)
        {
            var result= await collection.CreateIndex(key);
            Assert.NotEqual(result,string.Empty);
        }

        [Theory]
        [InlineData("category",1,false,"Index1")]
        public async void CreateIndex_WhenCreatedWithName_ShouldReturnSameIndexName(string key, int direction,  bool unique, 
                                                                string name)
        {
            var result= await collection.CreateIndex(key,direction,unique,name);
            Assert.Equal(result,name);
        }

        [Theory]
        [InlineData("category",1,false,"Index1")]
        public async void CreateIndex_WhenAlreadyExists_ShouldThrowException(string key, int direction,  bool unique, 
                                                                string name)
        {
            //Act
            Task result() => collection.CreateIndex(key,direction,unique,name);
            //Assert
            await Assert.ThrowsAsync<Exception>(result);
        }

    }
}
